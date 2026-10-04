import java.math.BigDecimal;
import java.net.URI;
import java.net.http.*;
import java.time.Duration;
import java.util.UUID;
import java.util.regex.*;
/** Ejecutar solo contra un esquema de prueba desechable. El token nunca se imprime. */
public final class JavaInventoryIntegrationTest {
    private static final String TOKEN = System.getenv("BODEGA_API_TOKEN");
    private static final HttpClient CLIENT = HttpClient.newBuilder().connectTimeout(Duration.ofSeconds(10)).build();
    private static final String PRODUCT = "P-001";
    public static void main(String[] args) throws Exception {
        if (TOKEN == null || TOKEN.isBlank()) throw new IllegalStateException("Configura BODEGA_API_TOKEN con una sesión de prueba; no lo escribas en el repositorio.");
        String url = args.length > 0 ? args[0] : System.getenv().getOrDefault("BODEGA_API_URL", "http://localhost:5248");
        URI base = URI.create(url.endsWith("/") ? url : url + "/");
        require(send(base.resolve("api/products"),"GET",null,false),401,"Acceso sin token");
        var profile=send(base.resolve("api/me"),"GET",null,true); require(profile,200,"Perfil");
        var initial=send(base.resolve("api/products"),"GET",null,true); require(initial,200,"Productos");
        var before=stock(initial.body(),PRODUCT);
        if(before.compareTo(BigDecimal.ONE)<0) throw new IllegalStateException("P-001 requiere una unidad en el ambiente de prueba.");
        String key=UUID.randomUUID().toString();
        String body="{\"paymentMethod\":\"Efectivo\",\"customer\":\"Prueba técnica\",\"requestId\":\""+key+"\",\"items\":[{\"productId\":\"P-001\",\"quantity\":1}]}";
        var saved=send(base.resolve("api/sales"),"POST",body,true);require(saved,200,"Venta");
        var repeated=send(base.resolve("api/sales"),"POST",body,true);require(repeated,200,"Reintento");
        if(!field(saved.body(),"id").equals(field(repeated.body(),"id"))) throw new AssertionError("El reintento duplicó la venta.");
        var after=send(base.resolve("api/products"),"GET",null,true);require(after,200,"Stock");
        if(stock(after.body(),PRODUCT).compareTo(before.subtract(BigDecimal.ONE))!=0) throw new AssertionError("El stock no se descontó una sola vez.");
        System.out.println("IT-API-01 OK: venta e idempotencia");
        String excess=body.replace(key,UUID.randomUUID().toString()).replace("\"quantity\":1","\"quantity\":999999");
        require(send(base.resolve("api/sales"),"POST",excess,true),400,"Exceso de stock");
        if(stock(send(base.resolve("api/products"),"GET",null,true).body(),PRODUCT).compareTo(stock(after.body(),PRODUCT))!=0) throw new AssertionError("La venta rechazada cambió el stock.");
        System.out.println("IT-API-02 OK: rechazo sin cambios");
        String id=field(saved.body(),"id");
        var cancelled=send(base.resolve("api/sales/"+id+"/cancel"),"POST","{\"reason\":\"Anulación de prueba\"}",true);
        if(profile.body().contains("\"role\":\"admin\"")) {
            require(cancelled,200,"Anulación admin");
            require(send(base.resolve("api/sales/"+id+"/cancel"),"POST","{\"reason\":\"Reintento\"}",true),200,"Reintento anulación");
            if(stock(send(base.resolve("api/products"),"GET",null,true).body(),PRODUCT).compareTo(before)!=0) throw new AssertionError("La anulación no restauró exactamente el stock.");
            System.out.println("IT-API-03 OK: anulación idempotente");
        } else { require(cancelled,403,"Cajero sin permiso de anular"); System.out.println("IT-API-03 OK: autorización de cajero"); }
    }
    private static HttpResponse<String> send(URI uri,String method,String body,boolean auth) throws Exception {
        var request=HttpRequest.newBuilder(uri).timeout(Duration.ofSeconds(90)).header("Accept","application/json");
        if(auth)request.header("Authorization","Bearer "+TOKEN);
        if(body!=null)request.header("Content-Type","application/json");
        request.method(method,body==null?HttpRequest.BodyPublishers.noBody():HttpRequest.BodyPublishers.ofString(body));
        return CLIENT.send(request.build(),HttpResponse.BodyHandlers.ofString());
    }
    private static void require(HttpResponse<String> response,int expected,String step) {
        if(response.statusCode()!=expected)throw new AssertionError(step+" devolvió "+response.statusCode()+"; esperado "+expected);
    }
    private static String field(String json,String name) {
        var m=Pattern.compile("\\\""+name+"\\\"\\s*:\\s*\\\"([^\\\"]+)\\\"").matcher(json);
        if(!m.find())throw new AssertionError("Falta "+name);
        return m.group(1);
    }
    private static BigDecimal stock(String json,String id) {
        var objects=Pattern.compile("\\{[^{}]*}").matcher(json);
        while(objects.find()){String o=objects.group();if(o.contains("\"id\":\""+id+"\"")){var m=Pattern.compile(Pattern.quote("\"stock\"")+"\\s*:\\s*(-?[0-9]+(?:\\.[0-9]+)?)").matcher(o);if(m.find())return new BigDecimal(m.group(1));}}
        throw new AssertionError("Falta stock de "+id);
    }
}
