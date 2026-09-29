using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace BodegaNorte.Api.Services;

public sealed class SupabaseBearerAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public SupabaseBearerAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorization = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrWhiteSpace(authorization))
            return AuthenticateResult.NoResult();

        if (!AuthenticationHeaderValue.TryParse(authorization, out var header) ||
            !string.Equals(header.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(header.Parameter))
        {
            return AuthenticateResult.Fail("A valid bearer token is required.");
        }

        var supabaseUrl = _configuration["Supabase:Url"]?.TrimEnd('/');
        var publishableKey = _configuration["Supabase:PublishableKey"];
        if (string.IsNullOrWhiteSpace(supabaseUrl) || string.IsNullOrWhiteSpace(publishableKey))
            return AuthenticateResult.Fail("Supabase Auth is not configured on the server.");

        using var request = new HttpRequestMessage(HttpMethod.Get, $"{supabaseUrl}/auth/v1/user");
        request.Headers.TryAddWithoutValidation("apikey", publishableKey);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", header.Parameter);

        try
        {
            using var response = await _httpClientFactory.CreateClient().SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                Context.RequestAborted);

            if (!response.IsSuccessStatusCode)
                return AuthenticateResult.Fail("Supabase did not accept the bearer token.");

            await using var content = await response.Content.ReadAsStreamAsync(Context.RequestAborted);
            using var user = await JsonDocument.ParseAsync(content, cancellationToken: Context.RequestAborted);
            var id = user.RootElement.TryGetProperty("id", out var idValue) ? idValue.GetString() : null;
            var email = user.RootElement.TryGetProperty("email", out var emailValue) ? emailValue.GetString() : null;
            if (!Guid.TryParse(id, out _))
                return AuthenticateResult.Fail("Supabase returned an invalid user identity.");

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, id!),
                new("sub", id!),
            };
            if (!string.IsNullOrWhiteSpace(email))
            {
                claims.Add(new Claim(ClaimTypes.Email, email));
                claims.Add(new Claim(ClaimTypes.Name, email));
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
        }
        catch (OperationCanceledException) when (!Context.RequestAborted.IsCancellationRequested)
        {
            Logger.LogWarning("Supabase Auth did not respond before the authentication request timed out.");
            return AuthenticateResult.Fail("Supabase Auth is temporarily unavailable.");
        }
        catch (HttpRequestException error)
        {
            Logger.LogWarning(error, "Could not validate the bearer token with Supabase Auth.");
            return AuthenticateResult.Fail("Supabase Auth is temporarily unavailable.");
        }
        catch (JsonException error)
        {
            Logger.LogWarning(error, "Supabase Auth returned an unreadable user response.");
            return AuthenticateResult.Fail("Supabase Auth returned an invalid response.");
        }
    }
}
