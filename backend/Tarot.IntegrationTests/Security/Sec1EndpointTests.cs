using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Npgsql;
using Tarot.Api.Security;
using Tarot.IntegrationTests.Infrastructure;

namespace Tarot.IntegrationTests.Security;

public sealed class Sec1EndpointTests(PostgreSqlFixture database)
    : IClassFixture<PostgreSqlFixture>, IAsyncLifetime
{
    private SecurityFactory _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await database.RecreateDatabaseAsync();
        _factory = new SecurityFactory(database.ConnectionString);
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://arcana.test"), AllowAutoRedirect = false
        });
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        Directory.Delete(_factory.KeyPath, recursive: true);
    }

    [Fact]
    public async Task AnonymousRequests_Get401ButCatalogueIsPublic()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync("/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync("/api/readings")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/api/spreads")).StatusCode);
    }

    [Fact]
    public async Task Login_UsesCodePkceStateNonceAndExactCallback()
    {
        var response = await _client.GetAsync("/auth/login?returnUrl=//evil.example");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(response.Headers.Location!.Query);
        Assert.Equal("code", query["response_type"].ToString());
        Assert.Equal("S256", query["code_challenge_method"].ToString());
        Assert.Equal(43, query["code_challenge"].ToString().Length);
        Assert.Equal("https://arcana.test/signin-oidc", query["redirect_uri"].ToString());
        Assert.False(string.IsNullOrEmpty(query["state"]));
        Assert.False(string.IsNullOrEmpty(query["nonce"]));
        Assert.False(query.ContainsKey("code_verifier"));
        Assert.False(query.ContainsKey("client_secret"));
        Assert.Equal("/", ArcanaSecurityOptions.SafeReturnUrl("//evil.example"));
        Assert.Equal("/", ArcanaSecurityOptions.SafeReturnUrl("/%5Cevil.example"));
    }

    [Fact]
    public async Task ExpiredSession_LoginClearsOldReferenceBeforeStartingOidc()
    {
        await _client.GetAsync("/test/login");
        var source = _factory.Services.GetRequiredService<NpgsqlDataSource>();
        await using (var command = source.CreateCommand(
            "UPDATE authentication_sessions SET expires_at_utc = now() - interval '1 minute'"))
            await command.ExecuteNonQueryAsync();
        var response = await _client.GetAsync("/auth/login?returnUrl=/account");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("keycloak.test", response.Headers.Location!.Host);
        Assert.Contains(response.Headers.GetValues("Set-Cookie"), value =>
            value.StartsWith("__Host-Arcana.Session=;") && value.Contains("expires="));
    }

    [Fact]
    public async Task CallbackWithoutValidState_DoesNotCreateSession()
    {
        await _client.GetAsync("/signin-oidc?code=fake&state=tampered");
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync("/auth/me")).StatusCode);
    }

    [Fact]
    public async Task Session_RequiresCsrfAndLogoutRevokesReplayedCookie()
    {
        // Test-only sign-in uses the real cookie handler and PostgreSQL store.
        // It does not claim to test Keycloak credential entry or code redemption.
        var login = await _client.GetAsync("/test/login");
        var cookie = login.Headers.GetValues("Set-Cookie")
            .Single(value => value.StartsWith("__Host-Arcana.Session="));
        Assert.Contains("httponly", cookie.ToLowerInvariant());
        Assert.Contains("secure", cookie.ToLowerInvariant());
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await _client.PostAsync("/auth/logout", null)).StatusCode);

        var csrf = await _client.GetAsync("/auth/csrf");
        using var body = JsonDocument.Parse(await csrf.Content.ReadAsStringAsync());
        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/logout");
        request.Headers.Add("X-CSRF-TOKEN", body.RootElement.GetProperty("token").GetString());
        var logout = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, logout.StatusCode);
        Assert.DoesNotContain("id_token_hint", await logout.Content.ReadAsStringAsync());

        using var replay = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://arcana.test"), AllowAutoRedirect = false, HandleCookies = false
        });
        replay.DefaultRequestHeaders.Add("Cookie", cookie.Split(';')[0]);
        Assert.Equal(HttpStatusCode.Unauthorized, (await replay.GetAsync("/auth/me")).StatusCode);
    }

    [Fact]
    public async Task ConcurrentRequests_RefreshOnceAndStoreOnlyEncryptedTickets()
    {
        var store = _factory.Services.GetRequiredService<PostgresTicketStore>();
        var ticket = TestLoginFilter.CreateTicket();
        var key = await store.StoreAsync(ticket);
        var source = _factory.Services.GetRequiredService<NpgsqlDataSource>();
        await using (var command = source.CreateCommand("SELECT protected_ticket FROM authentication_sessions LIMIT 1"))
        {
            var bytes = (byte[])(await command.ExecuteScalarAsync())!;
            Assert.DoesNotContain("test-refresh", Encoding.UTF8.GetString(bytes));
        }
        await using (var command = source.CreateCommand(
            "UPDATE authentication_sessions SET validated_at_utc = now() - interval '2 minutes'"))
            await command.ExecuteNonQueryAsync();

        var results = await Task.WhenAll(Enumerable.Range(0, 6).Select(_ => store.RetrieveAsync(key)));
        Assert.All(results, result => Assert.NotNull(result));
        Assert.Equal(1, _factory.RefreshHandler.Calls);
        Assert.All(results, result => Assert.Equal("rotated-refresh", result!.Properties.GetTokenValue("refresh_token")));
        await store.RemoveAsync(key);
        await store.RenewAsync(key, ticket);
        Assert.Null(await store.RetrieveAsync(key));
    }

    internal sealed class SecurityFactory(string connectionString) : WebApplicationFactory<Program>
    {
        public string KeyPath { get; } = Path.Combine(Path.GetTempPath(), "arcana-security-" + Guid.NewGuid());
        public FakeRefreshHandler RefreshHandler { get; } = new();
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:DefaultConnection", connectionString);
            builder.UseSetting("Database:ApplyMigrationsOnStartup", "false");
            builder.UseSetting("Security:Enabled", "true");
            builder.UseSetting("AiService:BaseUrl", "https://python.test");
            builder.UseSetting("Security:Authority", "https://keycloak.test/realms/arcana");
            builder.UseSetting("Security:PublicOrigin", "https://arcana.test");
            builder.UseSetting("Security:ClientSecret", new string('x', 64));
            builder.UseSetting("Security:DataProtectionKeyPath", KeyPath);
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IStartupFilter, TestLoginFilter>();
                services.Configure<OpenIdConnectOptions>(ArcanaSecurityOptions.OidcScheme, options =>
                    options.Configuration = new OpenIdConnectConfiguration
                    {
                        Issuer = "https://keycloak.test/realms/arcana",
                        AuthorizationEndpoint = "https://keycloak.test/authorize",
                        TokenEndpoint = "https://keycloak.test/token",
                        EndSessionEndpoint = "https://keycloak.test/logout"
                    });
                services.AddHttpClient("KeycloakRefresh").ConfigurePrimaryHttpMessageHandler(() => RefreshHandler);
            });
        }
    }

    internal sealed class FakeRefreshHandler : HttpMessageHandler
    {
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref Calls);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"access_token":"new-access","refresh_token":"rotated-refresh","expires_in":300}""",
                    Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class TestLoginFilter : IStartupFilter
    {
        public static AuthenticationTicket CreateTicket(string subject = "test-user")
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim("sub", subject, ClaimValueTypes.String, TestOwner.Value.Issuer), new Claim(ClaimTypes.NameIdentifier, subject)], "test"));
            var properties = new AuthenticationProperties();
            properties.StoreTokens([new AuthenticationToken { Name = "access_token", Value = "access-" + subject },
                new AuthenticationToken { Name = "refresh_token", Value = "test-refresh" }]);
            return new AuthenticationTicket(principal, properties, ArcanaSecurityOptions.CookieScheme);
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Use(async (context, proceed) =>
            {
                if (context.Request.Path == "/test/login" || context.Request.Path == "/test/login-second")
                {
                    var ticket = CreateTicket(context.Request.Path == "/test/login" ? "test-user" : "second-user");
                    await context.SignInAsync(ticket.AuthenticationScheme, ticket.Principal, ticket.Properties);
                    context.Response.StatusCode = 200;
                    return;
                }
                await proceed();
            });
            next(app);
        };
    }
}
