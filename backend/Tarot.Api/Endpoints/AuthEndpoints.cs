using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Tarot.Api.Security;

namespace Tarot.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/auth/login", async (HttpContext context, string? returnUrl) =>
        {
            var destination = ArcanaSecurityOptions.SafeReturnUrl(returnUrl);
            if (context.User.Identity?.IsAuthenticated == true)
                return Results.LocalRedirect(destination);
            // Clear an expired reference before leaving for Keycloak. The
            // callback then creates a fresh ticket instead of renewing a dead one.
            await context.SignOutAsync(ArcanaSecurityOptions.CookieScheme);
            return Results.Challenge(new AuthenticationProperties { RedirectUri = destination },
                [ArcanaSecurityOptions.OidcScheme]);
        }).AllowAnonymous();

        // This endpoint is used by the frontend to get the current user's information.
        endpoints.MapGet("/auth/me", (HttpContext context) => Results.Ok(new
        {
            subject = context.User.FindFirst("sub")?.Value, // account ID that /auth/me returned
            username = context.User.FindFirst("preferred_username")?.Value,
            name = context.User.FindFirst("name")?.Value,
            email = context.User.FindFirst("email")?.Value
        })).RequireAuthorization();

        endpoints.MapGet("/auth/csrf", (HttpContext context, IAntiforgery antiforgery) =>
            Results.Ok(new { token = antiforgery.GetAndStoreTokens(context).RequestToken }))
            .RequireAuthorization();

        endpoints.MapPost("/auth/logout", async (HttpContext context, IOptions<ArcanaSecurityOptions> settings) =>
        {
            // OIDC prepares the provider logout URL; cookie sign-out removes
            // the local session. The OIDC event omits tokens from that URL.
            var accountUrl = settings.Value.PublicOrigin.TrimEnd('/') + "/account";
            var logoutUrl = accountUrl;
            try
            {
                await context.SignOutAsync(ArcanaSecurityOptions.OidcScheme,
                    new AuthenticationProperties { RedirectUri = accountUrl });
                logoutUrl = context.Response.Headers.Location.ToString();
            }
            catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException or TaskCanceledException)
            {
                // Provider downtime must not stop local session deletion.
            }
            finally
            {
                await context.SignOutAsync(ArcanaSecurityOptions.CookieScheme);
            }
            context.Response.Headers.Remove("Location");
            context.Response.StatusCode = StatusCodes.Status200OK;
            return Results.Ok(new { logoutUrl });
        }).RequireAuthorization();
    }
}
