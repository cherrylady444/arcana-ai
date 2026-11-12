using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tarot.Application.Conversations;
using Tarot.Application.Interpretations;
using Tarot.IntegrationTests.Infrastructure;

namespace Tarot.IntegrationTests.Security;

public sealed class Sec2OwnershipTests(PostgreSqlFixture database) : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task TwoSessions_SeeOnlyTheirOwnData_AndDeniedActionsNeverReachAi()
    {
        await database.RecreateDatabaseAsync();
        var interpreter = new FakeReadingInterpreter();
        var responder = new CountingResponder();
        var host = new Sec1EndpointTests.SecurityFactory(database.ConnectionString);
        try
        {
            await using var factory = host.WithWebHostBuilder(b => b.ConfigureServices(s =>
            {
                s.AddSingleton<IReadingInterpreter>(interpreter);
                s.AddSingleton<IConversationResponder>(responder);
            }));
            var options = new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://arcana.test"), AllowAutoRedirect = false };
            using var ana = factory.CreateClient(options);
            using var ben = factory.CreateClient(options);
            Assert.Equal(HttpStatusCode.Unauthorized, (await ben.GetAsync("/api/readings")).StatusCode);
            await SignIn(ana, "/test/login");
            await SignIn(ben, "/test/login-second");

            var spread = new { spreadKey = "one-card", userContext = "practice" };
            var reading = await Id(await ana.PostAsJsonAsync("/api/readings", spread));
            var bensReading = await Id(await ben.PostAsJsonAsync("/api/readings", spread));
            await AssertOnly(ana, "/api/readings", reading);
            await AssertOnly(ben, "/api/readings", bensReading);

            var r = $"/api/readings/{reading}";
            var interpretation = await Id(await ana.PostAsync(r + "/interpretations", null));
            var conversation = await Id(await ana.PostAsJsonAsync("/api/conversations", new { message = "Discuss my reading.", interpretationId = interpretation }));
            var c = $"/api/conversations/{conversation}";
            await AssertOnly(ana, "/api/conversations", conversation);
            await AssertOnly(ben, "/api/conversations", null);

            foreach (var path in new[] { r, r + "/interpretations", r + "/interpretations/" + interpretation,
                                         "/api/interpretations/" + interpretation, c })
            {
                Assert.Equal(HttpStatusCode.OK, (await ana.GetAsync(path)).StatusCode);
                Assert.Equal(HttpStatusCode.NotFound, (await ben.GetAsync(path)).StatusCode);
            }

            foreach (var path in new[] { r + "/interpretations", c + "/interpretations", c + "/reading-recommendation" })
                Assert.Equal(HttpStatusCode.NotFound, (await ben.PostAsync(path, null)).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await ben.PostAsJsonAsync(c + "/readings", spread)).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await ben.PostAsJsonAsync(c + "/messages", new { message = "Another thought." })).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await ben.PostAsJsonAsync("/api/conversations", new { message = "Discuss.", readingId = reading })).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await ben.PostAsJsonAsync("/api/conversations", new { message = "Discuss.", interpretationId = interpretation })).StatusCode);
            Assert.Equal(1, interpreter.CallCount);
            Assert.Equal(1, responder.Calls);
            var details = await ana.GetFromJsonAsync<JsonElement>(c);
            Assert.Equal(2, details.GetProperty("messages").GetArrayLength());

            // Reproduce the owner fields of pre-SEC2 data in the disposable database.
            await using var db = database.CreateDbContext();
            await db.Readings.Where(x => x.Id == reading).ExecuteUpdateAsync(set => set
                .SetProperty(x => x.OwnerIssuer, (string?)null).SetProperty(x => x.OwnerSubject, (string?)null));
            await db.Conversations.Where(x => x.Id == conversation).ExecuteUpdateAsync(set => set
                .SetProperty(x => x.OwnerIssuer, (string?)null).SetProperty(x => x.OwnerSubject, (string?)null));
            await AssertOnly(ana, "/api/readings", null);
            await AssertOnly(ana, "/api/conversations", null);
            Assert.Equal(HttpStatusCode.NotFound, (await ana.GetAsync("/api/interpretations/" + interpretation)).StatusCode);
            Assert.True(await db.Readings.AnyAsync(x => x.Id == reading));

            // The allowed conversation -> draw -> interpretation path still works.
            var general = await Id(await ana.PostAsJsonAsync("/api/conversations", new { message = "Help me reflect." }));
            var fromChat = $"/api/conversations/{general}";
            var drawn = await Id(await ana.PostAsJsonAsync(fromChat + "/readings", new { spreadKey = "one-card" }));
            var linked = await ana.GetFromJsonAsync<JsonElement>(fromChat);
            Assert.Equal(drawn, linked.GetProperty("readingId").GetGuid());
            var generated = await Id(await ana.PostAsync(fromChat + "/interpretations", null));
            Assert.Equal(HttpStatusCode.NotFound, (await ben.GetAsync("/api/interpretations/" + generated)).StatusCode);
            await AssertOnly(ana, "/api/readings", drawn);
            await AssertOnly(ben, "/api/readings", bensReading);
        }
        finally
        {
            await host.DisposeAsync();
            if (Directory.Exists(host.KeyPath)) Directory.Delete(host.KeyPath, recursive: true);
        }
    }

    private static async Task SignIn(HttpClient client, string path)
    {
        (await client.GetAsync(path)).EnsureSuccessStatusCode();
        var csrf = await client.GetFromJsonAsync<JsonElement>("/auth/csrf");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf.GetProperty("token").GetString());
    }

    private static async Task<Guid> Id(HttpResponseMessage response)
    {
        using (response)
        {
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        }
    }

    private static async Task AssertOnly(HttpClient client, string path, Guid? expected)
    {
        var page = await client.GetFromJsonAsync<JsonElement>(path + "?page=1&pageSize=1");
        var items = page.GetProperty("items");
        Assert.Equal(expected.HasValue ? 1 : 0, page.GetProperty("totalCount").GetInt32());
        Assert.Equal(expected.HasValue ? 1 : 0, items.GetArrayLength());
        if (expected.HasValue) Assert.Equal(expected.Value, items[0].GetProperty("id").GetGuid());
    }

    private sealed class CountingResponder : IConversationResponder
    {
        public int Calls { get; private set; }
        public Task<ConversationResponderResult> RespondAsync(ConversationResponseInput input, CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(new ConversationResponderResult("Practice reply.", null));
        }
    }
}
