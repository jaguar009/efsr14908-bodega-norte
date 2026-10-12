using System.Text.Json;
using BodegaNorte.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BodegaNorte.Tools;
public static class AssistantVerification
{
    public static async Task RunAsync(string root,IConfiguration privateConfiguration,string reportPath)
    {
        var config=new ConfigurationBuilder().AddConfiguration(privateConfiguration)
            .AddInMemoryCollection(new Dictionary<string,string?>{{"Watson:Enabled","true"}}).Build();
        var services=new ServiceCollection();
        services.AddHttpClient("WatsonAssistant",client=>client.Timeout=TimeSpan.FromSeconds(15))
            .ConfigurePrimaryHttpMessageHandler(()=>new HttpClientHandler{AllowAutoRedirect=false});
        using var provider=services.BuildServiceProvider();
        var assistant=new WatsonAssistantService(config,provider.GetRequiredService<IHttpClientFactory>());
        if(!assistant.Configured)throw new InvalidOperationException("Falta la configuración privada del asistente IBM.");
        using var faq=JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(root,"integration","watson","preguntas_frecuentes.json")));
        var results=new List<object>();var actor=Guid.NewGuid();var passed=true;
        foreach(var entry in faq.RootElement.EnumerateArray())
        {
            var question=entry.GetProperty("question").GetString()!;
            var expected=entry.GetProperty("answer").GetString()!;
            var reply=await assistant.MessageAsync(question,actor,CancellationToken.None);
            var ok=reply.Text.Contains(expected[..Math.Min(80,expected.Length)],StringComparison.Ordinal);
            results.Add(new{question,response=reply.Text,provider=reply.Provider,status=ok?"aprobado":"falló"});
            Console.WriteLine("IBM REAL: "+question+" "+(ok?"OK":"FALLÓ"));passed&=ok;
        }
        var path=Path.GetFullPath(reportPath);Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path,JsonSerializer.Serialize(new{
            executedAt=DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-5)),
            environment="Instancia IBM real en plan Trial; API Assistant builder v2",
            mode="Ayuda operativa en español; no se envían registros de ventas ni productos",
            allPassed=passed,cases=results},new JsonSerializerOptions(JsonSerializerDefaults.Web){WriteIndented=true}));
        if(!passed)throw new InvalidOperationException("Revisa la clasificación o publicación del contenido IBM.");
    }
}
