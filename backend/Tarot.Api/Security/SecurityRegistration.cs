using Tarot.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Npgsql;

namespace Tarot.Api.Security;

public static class SecurityRegistration
{
    public static bool AddArcanaSecurity(this WebApplicationBuilder builder, string connectionString)
    {
        var enabled = builder.Configuration.GetValue("Security:Enabled", true);
        if (!enabled && !builder.Environment.IsEnvironment("Testing"))
            throw new InvalidOperationException("Security may be disabled only in the isolated Testing environment.");

        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();
        if (!enabled) return false;

        var settings = builder.Configuration.GetSection("Security").Get<ArcanaSecurityOptions>() ?? new();
        var issuer = settings.Authority.TrimEnd('/');
        ResourceOwner? ResolveOwner(System.Security.Claims.ClaimsPrincipal? user)
        {
            var subjects = user?.Identities.Where(identity => identity.IsAuthenticated)
                .SelectMany(identity => identity.FindAll("sub"))
                .Where(claim => claim.Issuer == issuer).Take(2).ToArray();
            return subjects is { Length: 1 } && !string.IsNullOrWhiteSpace(subjects[0].Value)
                ? new ResourceOwner(issuer, subjects[0].Value) : null;
        }

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ResourceOwner>(sp =>
            ResolveOwner(sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.User)
            ?? throw new InvalidOperationException("A validated account is required."));
        builder.Services.AddOptions<ArcanaSecurityOptions>()
            .Bind(builder.Configuration.GetSection("Security"))
            .Validate(o => o.IsValid(builder.Environment.IsDevelopment()),
                "Invalid Security configuration. Supply the client secret and trusted URLs; HTTP is permitted only for explicit loopback Development.")
            .ValidateOnStart();

        var keyDirectory = Path.GetFullPath(settings.DataProtectionKeyPath, builder.Environment.ContentRootPath);
        Directory.CreateDirectory(keyDirectory);
        if (!OperatingSystem.IsWindows())
            File.SetUnixFileMode(keyDirectory, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        builder.Services.AddDataProtection().SetApplicationName("ArcanaAI")
            .PersistKeysToFileSystem(new DirectoryInfo(keyDirectory));

        builder.Services.AddSingleton(_ => NpgsqlDataSource.Create(connectionString));
        builder.Services.AddSingleton<PostgresTicketStore>();
        builder.Services.AddSingleton<ITicketStore>(sp => sp.GetRequiredService<PostgresTicketStore>());
        builder.Services.AddHostedService<ExpiredSessionCleanup>();
        builder.Services.AddHttpClient("KeycloakRefresh", client => client.Timeout = TimeSpan.FromSeconds(10))
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });

        var loopbackHttp = builder.Environment.IsDevelopment() && settings.AllowLoopbackHttp;
        var cookieName = loopbackHttp ? "Arcana.Session" : "__Host-Arcana.Session";
        var securePolicy = loopbackHttp ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = ArcanaSecurityOptions.CookieScheme;
            // API challenges return 401; only /auth/login explicitly starts OIDC.
            options.DefaultChallengeScheme = ArcanaSecurityOptions.CookieScheme;
        })
        .AddCookie(ArcanaSecurityOptions.CookieScheme, options =>
        {
            options.Cookie.Name = cookieName;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = securePolicy;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.Path = "/";
            options.ExpireTimeSpan = TimeSpan.FromHours(settings.AbsoluteHours);
            options.SlidingExpiration = false;
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        })
        .AddOpenIdConnect(ArcanaSecurityOptions.OidcScheme, options =>
        {
            options.Authority = settings.Authority;
            options.RequireHttpsMetadata = !loopbackHttp;
            options.ClientId = settings.ClientId;
            options.ClientSecret = settings.ClientSecret;
            options.SignInScheme = ArcanaSecurityOptions.CookieScheme;
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.ResponseMode = OpenIdConnectResponseMode.Query;
            options.UsePkce = true;
            options.SaveTokens = true;
            options.UseTokenLifetime = false;
            options.MapInboundClaims = false;
            options.GetClaimsFromUserInfoEndpoint = false;
            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
            options.TokenValidationParameters.NameClaimType = "preferred_username";
            options.TokenValidationParameters.ValidateIssuer = true;
            options.TokenValidationParameters.ValidIssuer = settings.Authority.TrimEnd('/');
            options.CallbackPath = "/signin-oidc";
            options.SignedOutCallbackPath = "/signout-callback-oidc";
            // Keep the SEC-1 browser flow visible: no additional PAR transaction.
            options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Disable;
            options.CorrelationCookie.SecurePolicy = securePolicy;
            options.NonceCookie.SecurePolicy = securePolicy;
            if (loopbackHttp)
            {
                // query response mode returns via a top-level GET, compatible with Lax.
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                options.NonceCookie.SameSite = SameSiteMode.Lax;
            }
            options.Events.OnRedirectToIdentityProvider = context =>
            {
                context.ProtocolMessage.RedirectUri = settings.PublicOrigin.TrimEnd('/') + "/signin-oidc";
                return Task.CompletedTask;
            };
            options.Events.OnAuthorizationCodeReceived = context =>
            {
                // Must match the externally registered callback, even behind a proxy.
                context.TokenEndpointRequest!.RedirectUri = settings.PublicOrigin.TrimEnd('/') + "/signin-oidc";
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToIdentityProviderForSignOut = context =>
            {
                // Keep even the ID token out of browser URLs. Keycloak can use
                // client_id plus the registered redirect and show confirmation.
                context.ProtocolMessage.IdTokenHint = null;
                context.ProtocolMessage.ClientId = settings.ClientId;
                context.ProtocolMessage.PostLogoutRedirectUri = settings.PublicOrigin.TrimEnd('/') + "/signout-callback-oidc";
                return Task.CompletedTask;
            };
            options.Events.OnTicketReceived = async context =>
            {
                // A parallel login tab may already have established a session.
                // Do not renew/reuse its reference or overwrite its rotated tokens.
                if (context.Request.Cookies.ContainsKey(cookieName))
                {
                    var existing = await context.HttpContext.AuthenticateAsync(ArcanaSecurityOptions.CookieScheme);
                    if (!existing.Succeeded)
                        await context.HttpContext.SignOutAsync(ArcanaSecurityOptions.CookieScheme);
                    context.HandleResponse();
                    context.Response.Redirect(settings.PublicOrigin.TrimEnd('/')
                        + (existing.Succeeded ? "/account" : "/account?error=login"));
                    return;
                }
                context.Properties.IsPersistent = true;
                context.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddHours(settings.AbsoluteHours);
            };
            options.Events.OnRemoteFailure = context =>
            {
                context.HandleResponse();
                context.Response.Redirect(settings.PublicOrigin.TrimEnd('/') + "/account?error=login");
                return Task.CompletedTask;
            };
        });

        builder.Services.AddOptions<CookieAuthenticationOptions>(ArcanaSecurityOptions.CookieScheme)
            .Configure<ITicketStore>((options, store) => options.SessionStore = store);
        builder.Services.AddAuthorization(options => options.FallbackPolicy =
            new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
                .RequireAssertion(context => ResolveOwner(context.User) is not null).Build());
        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = loopbackHttp ? "Arcana.Antiforgery" : "__Host-Arcana.Antiforgery";
            options.Cookie.HttpOnly = true;
            options.Cookie.Path = "/";
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = securePolicy;
        });
        return true;
    }
}

internal sealed class ExpiredSessionCleanup(NpgsqlDataSource database, ILogger<ExpiredSessionCleanup> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(15));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var command = database.CreateCommand(
                    "DELETE FROM authentication_sessions WHERE expires_at_utc < now() OR absolute_expires_at_utc < now()");
                await command.ExecuteNonQueryAsync(stoppingToken);
            }
            catch (NpgsqlException) { logger.LogWarning("Expired session cleanup could not reach PostgreSQL."); }
        }
    }
}
