using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Tarot.IntegrationTests.Infrastructure;

namespace Tarot.IntegrationTests.Security;

public sealed class Sec3ExchangeTests(PostgreSqlFixture database) : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task OnlyDelegatedPassReachesPython_AndRejectedExchangeStopsThere()
    {
        await database.RecreateDatabaseAsync();
        var host = new Sec1EndpointTests.SecurityFactory(database.ConnectionString);
        var exchanges = 0;
        var calls = 0;
        try
        {
            await using var factory = host.WithWebHostBuilder(b => b.ConfigureServices(s =>
            {
                s.AddHttpClient("AiTokenExchange").ConfigurePrimaryHttpMessageHandler(() => new Probe(async req =>
                {
                    Assert.Equal("/realms/arcana/protocol/openid-connect/token", req.RequestUri!.AbsolutePath);
                    var fields = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(await req.Content!.ReadAsStringAsync());
                    Assert.Equal("access-test-user", fields["subject_token"].ToString());
                    Assert.Equal("arcana-ai", fields["audience"].ToString());
                    Assert.Equal("ai.invoke", fields["scope"].ToString());
                    Assert.Equal("urn:ietf:params:oauth:grant-type:token-exchange", fields["grant_type"].ToString());
                    exchanges++;
                    return exchanges == 1 ? new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("""{"token_type":"Bearer","issued_token_type":"urn:ietf:params:oauth:token-type:access_token","access_token":"ai-pass"}""", Encoding.UTF8, "application/json")
                    } : new HttpResponseMessage(HttpStatusCode.BadRequest);
                }));
                s.AddHttpClient("IReadingInterpreter").ConfigurePrimaryHttpMessageHandler(() => new Probe(req =>
                {
                    Assert.Equal("Bearer ai-pass", req.Headers.Authorization?.ToString());
                    Assert.False(req.Headers.Contains("Cookie"));
                    calls++;
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
                }));
            }));
            using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://arcana.test") });
            (await client.GetAsync("/test/login")).EnsureSuccessStatusCode();
            var csrf = await client.GetFromJsonAsync<JsonElement>("/auth/csrf");
            client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf.GetProperty("token").GetString());
            var saved = await client.PostAsJsonAsync("/api/readings", new { spreadKey = "one-card" });
            saved.EnsureSuccessStatusCode();
            var id = (await saved.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
            var path = $"/api/readings/{id}/interpretations";
            Assert.Equal(HttpStatusCode.ServiceUnavailable, (await client.PostAsync(path, null)).StatusCode);
            Assert.Equal(HttpStatusCode.ServiceUnavailable, (await client.PostAsync(path, null)).StatusCode);
            Assert.Equal(2, exchanges);
            Assert.Equal(1, calls);
        }
        finally
        {
            await host.DisposeAsync();
            if (Directory.Exists(host.KeyPath)) Directory.Delete(host.KeyPath, true);
        }
    }

    private sealed class Probe(Func<HttpRequestMessage, Task<HttpResponseMessage>> fn) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) => fn(request);
    }
}
