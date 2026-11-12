using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Tarot.Api.Security;

// A handler instance may be pooled. Never keep a user's token in a field.
public sealed class AiExchangeHandler(IHttpContextAccessor accessor, IHttpClientFactory clients,
    IOptions<ArcanaSecurityOptions> options) : DelegatingHandler
{
    private const string AccessToken = "urn:ietf:params:oauth:token-type:access_token";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var context = accessor.HttpContext;
        var source = context is null ? null :
            await context.GetTokenAsync(ArcanaSecurityOptions.CookieScheme, "access_token");
        if (string.IsNullOrWhiteSpace(source))
            throw new HttpRequestException("No authenticated session access token is available for AI.");

        var settings = options.Value;
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "urn:ietf:params:oauth:grant-type:token-exchange",
            ["client_id"] = settings.ClientId,
            ["client_secret"] = settings.ClientSecret,
            ["subject_token_type"] = AccessToken,
            ["subject_token"] = source,
            ["requested_token_type"] = AccessToken,
            ["audience"] = "arcana-ai",
            ["scope"] = "ai.invoke"
        });
        using var client = clients.CreateClient("AiTokenExchange");
        using var reply = await client.PostAsync(settings.Authority.TrimEnd('/') + "/protocol/openid-connect/token", form, ct);
        if (!reply.IsSuccessStatusCode)
            throw new HttpRequestException($"AI token exchange failed (HTTP {(int)reply.StatusCode}).");
        try
        {
            using var json = JsonDocument.Parse(await reply.Content.ReadAsStringAsync(ct));
            var root = json.RootElement;
            var token = root.GetProperty("access_token").GetString();
            if (string.IsNullOrWhiteSpace(token) || !string.Equals(root.GetProperty("token_type").GetString(),
                    "Bearer", StringComparison.OrdinalIgnoreCase) || root.GetProperty("issued_token_type").GetString() != AccessToken)
                throw new HttpRequestException("AI token exchange returned an unusable token.");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or KeyNotFoundException or FormatException)
        {
            throw new HttpRequestException("Invalid AI token exchange response.");
        }
        return await base.SendAsync(request, ct);
    }
}
