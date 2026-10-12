using Microsoft.AspNetCore.Antiforgery;

namespace Tarot.Api.Security;

public sealed class BrowserSecurityMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IAntiforgery antiforgery)
    {
        var path = context.Request.Path;
        // OIDC callbacks have protocol-level state/nonce checks. Application
        // mutations have an antiforgery check, including POSTs with no body.
        if ((path.StartsWithSegments("/api") || path.StartsWithSegments("/auth"))
            && context.User.Identity?.IsAuthenticated == true
            && !HttpMethods.IsGet(context.Request.Method)
            && !HttpMethods.IsHead(context.Request.Method)
            && !HttpMethods.IsOptions(context.Request.Method))
        {
            try { await antiforgery.ValidateRequestAsync(context); }
            catch (AntiforgeryValidationException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { error = "The security token is missing or invalid. Reload the page and try again." });
                return;
            }
        }
        await next(context);
    }
}
