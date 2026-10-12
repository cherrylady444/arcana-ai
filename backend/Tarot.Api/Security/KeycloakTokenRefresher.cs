using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Tarot.Api.Security;

public sealed class KeycloakTokenRefresher(HttpClient client, IOptions<ArcanaSecurityOptions> options)
{
    // false means Keycloak rejected the session. Transient failures throw and
    // fail this request closed without deleting a potentially valid session.
    public async Task<bool> RefreshAsync(AuthenticationTicket ticket, CancellationToken cancellationToken)
    {
        var refreshToken = ticket.Properties.GetTokenValue("refresh_token");
        if (string.IsNullOrEmpty(refreshToken)) return false;

        using var request = new HttpRequestMessage(HttpMethod.Post,
            options.Value.Authority.TrimEnd('/') + "/protocol/openid-connect/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = options.Value.ClientId,
                ["client_secret"] = options.Value.ClientSecret,
                ["refresh_token"] = refreshToken
            })
        };
        using var response = await client.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            using var error = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            if (error.RootElement.TryGetProperty("error", out var code)
                && code.GetString() == "invalid_grant") return false;
        }
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var root = document.RootElement;
        var accessToken = root.GetProperty("access_token").GetString();
        var expiresIn = root.GetProperty("expires_in").GetInt32();
        if (string.IsNullOrWhiteSpace(accessToken) || expiresIn <= 0)
            throw new HttpRequestException("Keycloak returned an invalid refresh response.");

        var tokens = ticket.Properties.GetTokens().ToDictionary(t => t.Name, t => t.Value);
        tokens["access_token"] = accessToken;
        tokens["expires_at"] = DateTimeOffset.UtcNow.AddSeconds(expiresIn).ToString("o", CultureInfo.InvariantCulture);
        if (root.TryGetProperty("refresh_token", out var rotated)
            && !string.IsNullOrWhiteSpace(rotated.GetString()))
            tokens["refresh_token"] = rotated.GetString()!;
        // Keep the validated login ID token server-side; refresh does not replace identity claims.
        ticket.Properties.StoreTokens(tokens.Select(t => new AuthenticationToken { Name = t.Key, Value = t.Value }));
        return true;
    }
}
