using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BodegaNorte.Api.Hosting;
using BodegaNorte.Api.Data;
using BodegaNorte.Api.Models;
using BodegaNorte.Api.Services;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace BodegaNorte.Tools;

public static class Verification
{
    private static readonly string[] Tables = ["categories", "suppliers", "products", "sales", "sale_items", "settings", "members", "stock_movements", "audit_log"];
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    public static async Task RunAsync(string root, string connection, string reportPath)
    {
        var schema = "bn_qa_" + Guid.NewGuid().ToString("N")[..16];
        var recoverySchema = "bodega_recovery_qa_" + Guid.NewGuid().ToString("N")[..12];
        var results = new List<object>();
        var baseline = await FingerprintAsync(connection, "bodega_norte");
        var schemaCreated = false;
        var recoveryCreated = false;
        var allPassed = false;
        await using var c = new NpgsqlConnection(connection);
        await c.OpenAsync();
        var identities = new[] { new FixtureIdentity("admin", "admin@qa.invalid", "admin", true),
            new FixtureIdentity("cashier", "cashier@qa.invalid", "cashier", true),
            new FixtureIdentity("inactive", "inactive@qa.invalid", "cashier", true, false),
            new FixtureIdentity("unconfirmed", "unconfirmed@qa.invalid", "admin", false) };
        string backupPath = Path.Combine(Path.GetTempPath(), "bodega-qa-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            foreach (var script in new[] { "schema.sql", "upgrade_operations.sql", "seed.sql" })
            {
                var sql = (await File.ReadAllTextAsync(Path.Combine(root, "database", script))).Replace("bodega_norte", schema, StringComparison.Ordinal);
                await using var command = new NpgsqlCommand(sql, c);
                await command.ExecuteNonQueryAsync();
                schemaCreated = true;
            }
            // Only the disposable schema uses identities provided by the local Auth fixture.
            // Production auth.users and memberships are neither inserted nor modified.
            await using (var fixture = new NpgsqlCommand($"ALTER TABLE \"{schema}\".members DROP CONSTRAINT members_user_id_fkey", c)) await fixture.ExecuteNonQueryAsync();
            foreach (var identity in identities)
            {
                await using var member = new NpgsqlCommand($"INSERT INTO \"{schema}\".members(user_id,email,role,is_active) VALUES(@id,@email,@role,@active)", c);
                member.Parameters.AddWithValue("id", identity.Id); member.Parameters.AddWithValue("email", identity.Email);
                member.Parameters.AddWithValue("role", identity.Role); member.Parameters.AddWithValue("active", identity.Active);
                await member.ExecuteNonQueryAsync();
            }
            var app = BodegaApplication.Create(["--environment", "Testing", "--contentRoot", Path.Combine(root,"backend","BodegaNorte.Api"), "--urls", "http://127.0.0.1:0"], builder =>
            {
                builder.Configuration.AddInMemoryCollection(new Dictionary<string,string?> {
                    ["ConnectionStrings:BodegaNorte"]=connection,["Database:Schema"]=schema,
                    ["Supabase:Url"]="https://auth-fixture.invalid",["Supabase:PublishableKey"]="fixture-publishable",
                    ["Watson:Enabled"]="true",["Watson:ServiceUrl"]="https://api.au-syd.assistant.watson.cloud.ibm.com/instances/qa",
                    ["Watson:AssistantId"]="10000000-0000-0000-0000-000000000001",["Watson:EnvironmentId"]="10000000-0000-0000-0000-000000000002",
                    ["Watson:ApiKey"]="isolated-fixture-key",
                    ["BootstrapAdminEmail"]="" });
                builder.Services.AddSingleton<IHttpClientFactory>(new FixtureHttpFactory(identities));
                builder.Logging.ClearProviders();
            });
            await using (app)
            {
                await app.StartAsync();
                var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
                using var client = new HttpClient { BaseAddress=new Uri(address),Timeout=TimeSpan.FromSeconds(40) };
                var admin = identities[0]; var cashier=identities[1];
                async Task<JsonElement> Send(string path, HttpMethod method, object? body = null, FixtureIdentity? person = null, int expected = 200)
                {
                    using var request = new HttpRequestMessage(method,path);
                    if (body is not null) request.Content=JsonContent.Create(body);
                    if (person is not null) request.Headers.Authorization=new("Bearer",person.Token);
                    using var response=await client.SendAsync(request);
                    if ((int)response.StatusCode != expected) throw new InvalidOperationException($"{method} {path}: HTTP {(int)response.StatusCode}; esperado {expected}.");
                    var raw=await response.Content.ReadAsStringAsync();
                    return string.IsNullOrWhiteSpace(raw)?JsonSerializer.SerializeToElement(new {}):JsonDocument.Parse(raw).RootElement.Clone();
                }
                async Task Check(string id, string description, Func<Task> action)
                {
                    try { await action(); results.Add(new {id,description,status="aprobado"});Console.WriteLine(id+" OK: "+description); }
                    catch (Exception error) { results.Add(new {id,description,status="falló",detail=error.Message});throw; }
                }
                async Task<JsonElement> Product(string code) => (await Send("/api/products",HttpMethod.Get,person:admin)).EnumerateArray().Single(p=>p.GetProperty("id").GetString()==code);
                async Task SetStock(string code, decimal stock)
                {
                    var p=await Product(code);var delta=stock-p.GetProperty("stock").GetDecimal();
                    if(delta!=0)await Send("/api/products/"+code+"/stock",HttpMethod.Post,new {quantity=delta,reason="Preparación de prueba aislada",expectedVersion=p.GetProperty("version").GetInt64()},admin);
                }
                object Sale(Guid request, decimal quantity=1,string code="P-001")=>new {paymentMethod="Efectivo",customer="Prueba aislada",requestId=request,items=new[]{new {productId=code,quantity}}};
                await Check("HTTP-01","Disponibilidad y esquema migrado",async()=>{await Send("/api/health/ready",HttpMethod.Get);});
                await Check("HTTP-02","Readiness rechaza un esquema que no está instalado",async()=>{
                    var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"Database:Schema","bn_qa_missing_"+Guid.NewGuid().ToString("N")[..12]}}).Build();
                    await using var missing=new BodegaRepository(connection,config);
                    Ensure(!await missing.TestConnectionAsync(CancellationToken.None),"Se aceptó un esquema sin tablas.");
                });
                await Check("AUTH-01","Acceso anónimo e identidades sin confirmar rechazados",async()=>{
                    await Send("/api/products",HttpMethod.Get,expected:401);
                    await Send("/api/products",HttpMethod.Get,person:identities[3],expected:401);
                    await Send("/api/products",HttpMethod.Get,person:identities[2],expected:403);
                });
                await Check("AUTH-02","Rol leído de members e intento de elevar user_metadata rechazado",async()=>{
                    var me=await Send("/api/me",HttpMethod.Get,person:cashier);
                    Ensure(me.GetProperty("role").GetString()=="cashier","Se aceptó un rol del metadata del cliente.");
                    await Send("/api/backup",HttpMethod.Get,person:cashier,expected:403);
                    await Send("/api/settings",HttpMethod.Put,new {name="No autorizado",ruc="",address="",phone="",stockAlerts=true},cashier,403);
                });
                await Check("IBM-01","Contrato HTTP de Watson sin exponer credenciales ni registros",async()=>{
                    await Send("/api/assistant/message",HttpMethod.Post,new {message="Cómo registro una venta"},expected:401);
                    var status=await Send("/api/assistant/status",HttpMethod.Get,person:admin);
                    Ensure(status.GetProperty("configured").GetBoolean(),"No se reconoció la configuración aislada.");
                    var reply=await Send("/api/assistant/message",HttpMethod.Post,new {message="Cómo registro una venta"},cashier);
                    Ensure(reply.GetProperty("provider").GetString()=="watson"&&reply.GetProperty("text").GetString()=="Respuesta simulada de IBM para prueba de contrato.","Respuesta inesperada.");
                    await Send("/api/assistant/message",HttpMethod.Post,new {message=new string('x',501)},admin,400);
                });
                await Check("IBM-02","Configuración ausente o URL ajena a IBM no envía consultas",async()=>{
                    var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{
                        ["Watson:Enabled"]="true",["Watson:ServiceUrl"]="https://untrusted.invalid",["Watson:ApiKey"]="fixture",
                        ["Watson:AssistantId"]="10000000-0000-0000-0000-000000000001"}).Build();
                    var service=new WatsonAssistantService(config,new FixtureHttpFactory(identities));
                    Ensure(!service.Configured,"Se aceptó un destino ajeno a IBM.");
                    try{await service.MessageAsync("ayuda",admin.Id,CancellationToken.None);throw new InvalidOperationException("Configuración inválida aceptada.");}
                    catch(BusinessException error)when(error.Status==503){}
                });
                await Check("IBM-03","Límite de consultas protege la cuota del asistente",async()=>{
                    // The earlier invalid request consumes one permit before MVC validation.
                    for(var i=0;i<19;i++)await Send("/api/assistant/message",HttpMethod.Post,new {message="ayuda"},admin);
                    await Send("/api/assistant/message",HttpMethod.Post,new {message="ayuda"},admin,429);
                });
                await Check("DATA-01","Creación edición y archivo de producto con auditoría",async()=>{
                    var input=new {name="Producto QA",category="QA",stock=4,minStock=1,cost=2,price=3,supplier="",unit="unidad"};
                    var p=await Send("/api/products",HttpMethod.Post,input,admin);var id=p.GetProperty("id").GetString()!;
                    await Send("/api/products/"+id,HttpMethod.Put,new {name="Producto QA actualizado",category="QA",stock=4,minStock=1,cost=2,price=3,supplier="",unit="unidad",expectedVersion=p.GetProperty("version").GetInt64()},admin);
                    await Send("/api/products/"+id,HttpMethod.Delete,person:admin,expected:204);
                    Ensure(!(await Send("/api/products",HttpMethod.Get,person:admin)).EnumerateArray().Any(p=>p.GetProperty("id").GetString()==id),"Producto archivado todavía activo.");
                });
                await Check("SALE-01","Venta válida reintento y descuento de stock una vez",async()=>{
                    await SetStock("P-001",3);var key=Guid.NewGuid();var one=await Send("/api/sales",HttpMethod.Post,Sale(key),admin);
                    var two=await Send("/api/sales",HttpMethod.Post,Sale(key),admin);
                    Ensure(one.GetProperty("id").GetString()==two.GetProperty("id").GetString(),"Venta duplicada.");
                    Ensure((await Product("P-001")).GetProperty("stock").GetDecimal()==2,"Descuento de stock incorrecto.");
                    await Send("/api/sales",HttpMethod.Post,Sale(key,2),admin,409);
                    await Send("/api/sales",HttpMethod.Post,Sale(Guid.NewGuid(),999999),admin,400);
                    Ensure((await Product("P-001")).GetProperty("stock").GetDecimal()==2,"La operación rechazada cambió stock.");
                    var saleId=one.GetProperty("id").GetString()!;
                    await Send("/api/sales/"+saleId+"/cancel",HttpMethod.Post,new {reason="Prueba"},cashier,403);
                    await Send("/api/sales/"+saleId+"/cancel",HttpMethod.Post,new {reason="Prueba"},admin);
                    await Send("/api/sales/"+saleId+"/cancel",HttpMethod.Post,new {reason="Reintento"},admin);
                    Ensure((await Product("P-001")).GetProperty("stock").GetDecimal()==3,"Anulación no idempotente.");
                });
                await Check("SALE-02","Reintentos concurrentes conservan una única venta",async()=>{
                    await SetStock("P-001",3);var key=Guid.NewGuid();
                    var responses=await Task.WhenAll(Enumerable.Range(0,4).Select(_=>Send("/api/sales",HttpMethod.Post,Sale(key),admin)));
                    Ensure(responses.Select(x=>x.GetProperty("id").GetString()).Distinct().Count()==1,"Reintento concurrente duplicado.");
                    Ensure((await Product("P-001")).GetProperty("stock").GetDecimal()==2,"Stock descontado más de una vez.");
                });
                await Check("SALE-03","Dos ventas concurrentes compiten por la última unidad",async()=>{
                    await SetStock("P-001",1);
                    async Task<int> Compete(){using var request=new HttpRequestMessage(HttpMethod.Post,"/api/sales"){Content=JsonContent.Create(Sale(Guid.NewGuid()))};request.Headers.Authorization=new("Bearer",admin.Token);using var response=await client.SendAsync(request);return (int)response.StatusCode;}
                    var statuses=await Task.WhenAll(Compete(),Compete());
                    Ensure(statuses.Count(x=>x==200)==1&&statuses.Count(x=>x==400)==1,"La última unidad se vendió dos veces.");
                    Ensure((await Product("P-001")).GetProperty("stock").GetDecimal()==0,"Stock concurrente incorrecto.");
                });
                await Check("STOCK-01","Ajustes concurrentes y versión desactualizada",async()=>{
                    await SetStock("P-001",3);var p=await Product("P-001");
                    async Task<int> Adjust(){using var request=new HttpRequestMessage(HttpMethod.Post,"/api/products/P-001/stock"){Content=JsonContent.Create(new{quantity=1,reason="Concurrencia QA",expectedVersion=p.GetProperty("version").GetInt64()})};request.Headers.Authorization=new("Bearer",admin.Token);using var response=await client.SendAsync(request);return(int)response.StatusCode;}
                    var statuses=await Task.WhenAll(Adjust(),Adjust());
                    Ensure(statuses.Count(x=>x==200)==1&&statuses.Count(x=>x==409)==1,"Ambos ajustes ignoraron la versión.");
                    Ensure((await Product("P-001")).GetProperty("stock").GetDecimal()==4,"Saldo de ajuste incorrecto.");
                });
                await Check("DATA-02","Proveedor con productos no puede eliminarse y configuración persiste",async()=>{
                    var suppliers=await Send("/api/suppliers",HttpMethod.Get,person:admin);
                    var used=suppliers.EnumerateArray().First(p=>p.GetProperty("products").GetInt32()>0);
                    await Send("/api/suppliers/"+used.GetProperty("id").GetInt32(),HttpMethod.Delete,person:admin,expected:409);
                    await Send("/api/settings",HttpMethod.Put,new {name="Bodega QA",ruc="",address="Entorno aislado",phone="",stockAlerts=false},admin,204);
                    var settings=await Send("/api/settings",HttpMethod.Get,person:cashier);
                    Ensure(settings.GetProperty("name").GetString()=="Bodega QA"&&!settings.GetProperty("stockAlerts").GetBoolean(),"Configuración no persistida.");
                    await Send("/api/members",HttpMethod.Put,new {email=admin.Email,role="cashier",active=false},admin,400);
                });
                await Check("JAVA-01","Programa Java ejecutado con rol administrador",async()=>{
                    await SetStock("P-001",3);
                    var classes=Path.Combine(Path.GetTempPath(),"bodega-java-"+schema);Directory.CreateDirectory(classes);
                    await RunProcess("javac",["-encoding","UTF-8","-d",classes,Path.Combine(root,"integration","JavaInventoryIntegrationTest.java")]);
                    await RunProcess("java",["-Dfile.encoding=UTF-8","-Dstdout.encoding=UTF-8","-cp",classes,"JavaInventoryIntegrationTest",address],admin.Token);
                });
                await Check("BACKUP-01","Exportación consistente y recuperación en un esquema nuevo",async()=>{
                    var backup=await Send("/api/backup",HttpMethod.Get,person:admin);
                    await File.WriteAllTextAsync(backupPath,backup.GetRawText());
                    var restored=await Recovery.RestoreAsync(root,connection,backupPath,recoverySchema);recoveryCreated=true;
                    Ensure(restored.Rows["products"]==backup.GetProperty("data").GetProperty("products").GetArrayLength(),"Conteo de catálogo recuperado incorrecto.");
                    Ensure(await FingerprintTableAsync(connection,schema,"products")==await FingerprintTableAsync(connection,recoverySchema,"products"),"Catálogo recuperado diferente.");
                    Ensure(await FingerprintTableAsync(connection,schema,"sales")==await FingerprintTableAsync(connection,recoverySchema,"sales"),"Ventas recuperadas diferentes.");
                    Ensure(restored.MembersNotRestored==identities.Length,"La recuperación debe separar los permisos.");
                    try{await Recovery.RestoreAsync(root,connection,backupPath,recoverySchema);throw new InvalidOperationException("Se sobreescribió un esquema existente.");}catch(InvalidOperationException error)when(error.Message.StartsWith("El esquema ya existe")){}
                    try{await Recovery.RestoreAsync(root,connection,backupPath,"bodega_norte");throw new InvalidOperationException("Se aceptó la base principal.");}catch(InvalidOperationException error)when(error.Message.StartsWith("Usa un esquema nuevo")){}
                });
                await Check("AUDIT-01","Ventas ajustes y permisos administrativos dejan trazabilidad",async()=>{
                    var audit=await Send("/api/audit",HttpMethod.Get,person:admin);
                    Ensure(audit.GetArrayLength()>0,"No se generó auditoría.");
                    var movements=await Send("/api/movements",HttpMethod.Get,person:cashier);
                    Ensure(movements.EnumerateArray().Any(m=>m.GetProperty("type").GetString()=="cancellation"),"Falta la devolución auditada.");
                });
                await Check("BACKUP-02","Un respaldo con datos inválidos revierte toda la recuperación",async()=>{
                    var broken=System.Text.Json.Nodes.JsonNode.Parse(await File.ReadAllTextAsync(backupPath))!;
                    broken["data"]!["products"]![0]!["stock"]=-1;
                    var badPath=backupPath+".invalid.json";await File.WriteAllTextAsync(badPath,broken.ToJsonString());
                    var badSchema=recoverySchema+"_invalid";
                    try{await Recovery.RestoreAsync(root,connection,badPath,badSchema);throw new InvalidOperationException("Se aceptó un stock negativo.");}
                    catch(PostgresException error)when(error.SqlState=="23514"){}
                    await using var exists=new NpgsqlCommand("SELECT EXISTS(SELECT FROM pg_namespace WHERE nspname=@schema)",c);
                    exists.Parameters.AddWithValue("schema",badSchema);
                    Ensure(!(bool)(await exists.ExecuteScalarAsync())!,"La recuperación fallida dejó un esquema parcial.");
                });
                await app.StopAsync();
                allPassed=true;
            }
        }
        finally
        {
            if(recoveryCreated)await DropOwnedSchema(c,recoverySchema,"bodega_recovery_qa_");
            if(schemaCreated)await DropOwnedSchema(c,schema,"bn_qa_");
            var final=await FingerprintAsync(connection,"bodega_norte");
            var unchanged=baseline==final;
            var path=Path.GetFullPath(reportPath);Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllTextAsync(path,JsonSerializer.Serialize(new {
                executedAt=DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-5)),
                environment="HTTP Kestrel local y PostgreSQL real en esquema desechable",
                auth="Respuesta de Supabase Auth sustituida por fixture local; autorización real desde members",
                ibm="Respuesta de IBM sustituida por fixture local; estas pruebas no demuestran conexión con una instancia IBM real",
                qaSchema=schema,recoverySchema,cleanedUp=true,productionUnchanged=unchanged,
                productionFingerprintBefore=baseline,productionFingerprintAfter=final,
                allPassed=allPassed&&unchanged,cases=results },Json));
            Console.WriteLine("BASE PRINCIPAL: "+(unchanged?"sin cambios":"cambió durante la ejecución"));
            Console.WriteLine("REGISTRO: "+path);
            if(!unchanged)throw new InvalidOperationException("La base principal cambió durante la ejecución; revisa el registro.");
        }
    }
    private static void Ensure(bool value,string message){if(!value)throw new InvalidOperationException(message);}
    private static async Task<string> FingerprintAsync(string connection,string schema)
    {
        var hashes=new List<string>();foreach(var table in Tables)hashes.Add(await FingerprintTableAsync(connection,schema,table));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(',',hashes))));
    }
    private static async Task<string> FingerprintTableAsync(string connection,string schema,string table)
    {
        await using var c=new NpgsqlConnection(connection);await c.OpenAsync();
        await using var command=new NpgsqlCommand($"SELECT COALESCE(jsonb_agg(to_jsonb(t) ORDER BY to_jsonb(t)::text),'[]'::jsonb)::text FROM \"{schema}\".{table} t",c);
        var value=(string)(await command.ExecuteScalarAsync())!;
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }
    private static async Task DropOwnedSchema(NpgsqlConnection c,string schema,string prefix)
    {
        if(!schema.StartsWith(prefix,StringComparison.Ordinal)||!System.Text.RegularExpressions.Regex.IsMatch(schema,"^[a-z0-9_]+$"))throw new InvalidOperationException("Destino de limpieza inválido.");
        await using var command=new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE",c);await command.ExecuteNonQueryAsync();
    }
    private static async Task RunProcess(string program,string[] arguments,string? token=null)
    {
        var start=new ProcessStartInfo(program){RedirectStandardOutput=true,RedirectStandardError=true,StandardOutputEncoding=Encoding.UTF8,StandardErrorEncoding=Encoding.UTF8,UseShellExecute=false,CreateNoWindow=true};
        foreach(var argument in arguments)start.ArgumentList.Add(argument);
        if(token is not null)start.Environment["BODEGA_API_TOKEN"]=token;
        using var process=Process.Start(start)??throw new InvalidOperationException("No se pudo iniciar "+program);
        var outputTask=process.StandardOutput.ReadToEndAsync();var errorTask=process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();var output=await outputTask;var error=await errorTask;
        if(process.ExitCode!=0)throw new InvalidOperationException(program+" falló: "+error.Trim());
        if(!string.IsNullOrWhiteSpace(output))Console.Write(output);
    }
    private sealed class FixtureIdentity(string label,string email,string role,bool confirmed,bool active=true)
    {
        public Guid Id{get;}=Guid.NewGuid();public string Token{get;}="qa-"+label+"-"+Guid.NewGuid().ToString("N");
        public string Email{get;}=email;public string Role{get;}=role;public bool Confirmed{get;}=confirmed;public bool Active{get;}=active;
    }
    private sealed class FixtureHttpFactory(FixtureIdentity[] identities):IHttpClientFactory
    {
        public HttpClient CreateClient(string name)=>new(new FixtureHandler(identities));
    }
    private sealed class FixtureHandler(FixtureIdentity[] identities):HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            if(request.RequestUri?.Host=="iam.cloud.ibm.com")
            {
                Ensure((await request.Content!.ReadAsStringAsync(ct)).Contains("apikey=isolated-fixture-key"),"Clave IBM no enviada al endpoint IAM.");
                return new HttpResponseMessage(HttpStatusCode.OK){Content=JsonContent.Create(new {access_token="fixture-iam-token",expires_in=3600})};
            }
            if(request.RequestUri?.Host=="api.au-syd.assistant.watson.cloud.ibm.com")
            {
                Ensure(request.RequestUri.AbsolutePath=="/instances/qa/v2/assistants/10000000-0000-0000-0000-000000000001/environments/10000000-0000-0000-0000-000000000002/message","Ruta Watson incorrecta.");
                Ensure(request.Headers.Authorization?.Scheme=="Bearer"&&request.Headers.Authorization.Parameter=="fixture-iam-token","Autenticación IBM incorrecta.");
                using var body=JsonDocument.Parse(await request.Content!.ReadAsStringAsync(ct));
                Ensure(body.RootElement.EnumerateObject().Select(x=>x.Name).Order().SequenceEqual(new[]{"input","user_id"}),"La consulta envió campos adicionales.");
                Ensure(body.RootElement.GetProperty("user_id").GetString()!.Length==64,"Identificador IBM no seudonimizado.");
                return new HttpResponseMessage(HttpStatusCode.OK){Content=JsonContent.Create(new {output=new {generic=new[]{new {response_type="text",text="Respuesta simulada de IBM para prueba de contrato."}}}})};
            }
            var identity=identities.SingleOrDefault(i=>i.Token==request.Headers.Authorization?.Parameter);
            if(request.RequestUri?.Host!="auth-fixture.invalid"||identity is null)return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            return new HttpResponseMessage(HttpStatusCode.OK){Content=JsonContent.Create(new {
                id=identity.Id,email=identity.Email,email_confirmed_at=identity.Confirmed?"2026-10-11T00:00:00Z":null,
                user_metadata=new{role="admin"} })};
        }
    }
}
