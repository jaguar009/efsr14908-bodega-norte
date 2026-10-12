using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BodegaNorte.Api.Models;

namespace BodegaNorte.Api.Services;

// One-turn operational help. The server never sends database records or executes
// transactions from an assistant reply. Enable only after checking the IBM plan.
public sealed class WatsonAssistantService(IConfiguration configuration, IHttpClientFactory clients)
{
    private readonly SemaphoreSlim tokenLock = new(1, 1);
    private string? token;
    private DateTimeOffset tokenExpires;
    public bool Configured => Endpoint() is not null &&
        !string.IsNullOrWhiteSpace(configuration["Watson:ApiKey"]) &&
        configuration.GetValue<bool>("Watson:Enabled");

    private Uri? Endpoint()
    {
        if (!Uri.TryCreate(configuration["Watson:ServiceUrl"], UriKind.Absolute, out var url) ||
            url.Scheme != "https" || !url.IsDefaultPort || !string.IsNullOrEmpty(url.UserInfo) ||
            !string.IsNullOrEmpty(url.Query) || !string.IsNullOrEmpty(url.Fragment) ||
            !(url.Host.EndsWith(".assistant.watson.cloud.ibm.com", StringComparison.OrdinalIgnoreCase) ||
              url.Host.EndsWith(".assistant-builder.watson.cloud.ibm.com", StringComparison.OrdinalIgnoreCase) ||
              url.Host.EndsWith(".assistant.watsonx.cloud.ibm.com", StringComparison.OrdinalIgnoreCase) ||
              (url.Host.StartsWith("api.",StringComparison.OrdinalIgnoreCase) && url.Host.EndsWith(".watson-orchestrate.cloud.ibm.com", StringComparison.OrdinalIgnoreCase)))) return null;
        var assistant = configuration["Watson:AssistantId"];
        var environment = configuration["Watson:EnvironmentId"];
        if (!Guid.TryParse(assistant, out var id)) return null;
        var path = $"/v2/assistants/{id:D}";
        if (!string.IsNullOrWhiteSpace(environment))
        {
            if (!Guid.TryParse(environment, out var env)) return null;
            path += $"/environments/{env:D}";
        }
        return new Uri(url.AbsoluteUri.TrimEnd('/') + path + "/message?version=2024-08-25");
    }

    public async Task<AssistantReply> MessageAsync(string message, Guid actorId, CancellationToken ct)
    {
        message = message.Trim();
        if (message.Length is < 1 or > 500) throw new BusinessException("Escribe una consulta de 1 a 500 caracteres.");
        if (!Configured) throw new BusinessException("Watson todavía no está habilitado. Usa la ayuda local mientras se configura la cuenta IBM.", 503);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(15));
        var requestToken = timeout.Token;
        var client = clients.CreateClient("WatsonAssistant");
        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint());
        request.Headers.Add("X-Watson-Learning-Opt-Out", "true");
        request.Content = JsonContent.Create(new {
            input = new { message_type = "text", text = message },
            user_id = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(actorId.ToString("D"))))
        });
        try
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await TokenAsync(requestToken));
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, requestToken);
            if (!response.IsSuccessStatusCode) throw new BusinessException("IBM no pudo responder. Comprueba la instancia, el plan y las credenciales; puedes usar la ayuda local.", 503);
            // Bound the complete response, including chunked transfers.
            using var payload = new MemoryStream();
            await using var stream = await response.Content.ReadAsStreamAsync(requestToken);
            var buffer = new byte[8192]; int read;
            while ((read = await stream.ReadAsync(buffer, requestToken)) != 0)
            {
                if (payload.Length + read > 131072) throw new BusinessException("La respuesta de IBM excedió el límite. Reintenta con una consulta breve.", 503);
                await payload.WriteAsync(buffer.AsMemory(0, read), requestToken);
            }
            using var json = JsonDocument.Parse(payload.ToArray());
            var lines = json.RootElement.GetProperty("output").GetProperty("generic").EnumerateArray()
                .Where(x => x.TryGetProperty("response_type", out var type) && type.GetString() == "text")
                .Select(x => x.GetProperty("text").GetString()).Where(x => !string.IsNullOrWhiteSpace(x));
            var text = string.Join("\n", lines);
            if (text.Length == 0) throw new BusinessException("Watson no devolvió una respuesta de texto. Revisa las acciones del asistente.", 503);
            if (text.Length > 8000) text = text[..8000];
            return new AssistantReply(text, "watson");
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        { throw new BusinessException("Watson tardó en responder. Reintenta o usa la ayuda local.", 503); }
        catch (HttpRequestException) { throw new BusinessException("No se pudo conectar con IBM. Usa la ayuda local e inténtalo después.", 503); }
        catch (Exception error) when (error is JsonException or InvalidOperationException or KeyNotFoundException)
        { throw new BusinessException("La respuesta de IBM no tiene el formato esperado. Revisa la configuración.", 503); }
    }
    private async Task<string> TokenAsync(CancellationToken ct)
    {
        await tokenLock.WaitAsync(ct);
        try
        {
            if (token is not null && DateTimeOffset.UtcNow < tokenExpires) return token;
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://iam.cloud.ibm.com/identity/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string,string> {
                    ["grant_type"] = "urn:ibm:params:oauth:grant-type:apikey",
                    ["apikey"] = configuration["Watson:ApiKey"]!
                })
            };
            using var response = await clients.CreateClient("WatsonAssistant").SendAsync(request,ct);
            if (!response.IsSuccessStatusCode) throw new BusinessException("IBM rechazó la credencial. Revisa la clave privada del servicio.",503);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
            token = json.RootElement.GetProperty("access_token").GetString();
            var duration = json.RootElement.GetProperty("expires_in").GetInt32();
            if(string.IsNullOrWhiteSpace(token)||duration<1)throw new BusinessException("IBM no devolvió una credencial temporal válida.",503);
            tokenExpires = DateTimeOffset.UtcNow.AddSeconds(Math.Max(1,duration-60));
            return token;
        }
        finally { tokenLock.Release(); }
    }
}
public sealed record AssistantReply(string Text, string Provider);
