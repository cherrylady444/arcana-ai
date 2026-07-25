using System.Net;
using System.Net.Http.Json;
using Tarot.Api.Contracts;
using Tarot.IntegrationTests.Infrastructure;

namespace Tarot.IntegrationTests.Api;

/// <summary>
/// Exercises the built-in spread catalogue through the real HTTP boundary.
/// </summary>
public sealed class SpreadEndpointTests :
    IClassFixture<PostgreSqlFixture>,
    IDisposable
{
    private readonly TarotApiFactory _factory;
    private readonly HttpClient _client;

    public SpreadEndpointTests(PostgreSqlFixture fixture)
    {
        _factory = new TarotApiFactory(fixture.ConnectionString);
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetSpreads_ReturnsAvailableBuiltInSpreads()
    {
        var response = await _client.GetAsync("/api/spreads");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var spreads = await response.Content
            .ReadFromJsonAsync<List<SpreadSummaryResponse>>();

        Assert.NotNull(spreads);
        Assert.NotEmpty(spreads);

        Assert.All(spreads, spread =>
        {
            Assert.False(string.IsNullOrWhiteSpace(spread.Key));
            Assert.False(string.IsNullOrWhiteSpace(spread.Name));
            Assert.False(string.IsNullOrWhiteSpace(spread.ShortDescription));
            Assert.False(string.IsNullOrWhiteSpace(spread.Intent));
            Assert.True(spread.CardCount > 0);
        });

        Assert.Contains(
            spreads,
            spread => spread.Key == "five-card");
    }

    [Fact]
    public async Task GetSpreadByKey_WithValidKey_ReturnsOrderedPositions()
    {
        var response = await _client.GetAsync("/api/spreads/five-card");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var spread = await response.Content
            .ReadFromJsonAsync<SpreadDetailResponse>();

        Assert.NotNull(spread);
        Assert.Equal("five-card", spread.Key);
        Assert.False(string.IsNullOrWhiteSpace(spread.Name));
        Assert.NotEmpty(spread.Positions);
        Assert.Equal(spread.CardCount, spread.Positions.Count);

        var returnedOrder = spread.Positions
            .Select(position => position.Order)
            .ToArray();

        Assert.Equal(
            returnedOrder.OrderBy(order => order).ToArray(),
            returnedOrder);

        Assert.All(spread.Positions, position =>
        {
            Assert.True(position.Order > 0);
            Assert.False(string.IsNullOrWhiteSpace(position.Key));
            Assert.False(string.IsNullOrWhiteSpace(position.Title));
            Assert.False(string.IsNullOrWhiteSpace(position.Prompt));
            Assert.False(string.IsNullOrWhiteSpace(position.Description));
        });
    }

    [Fact]
    public async Task GetSpreadByKey_WithUnknownKey_ReturnsNotFound()
    {
        var response = await _client.GetAsync(
            "/api/spreads/non-existent-spread");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var error = await response.Content
            .ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(error);
        Assert.Contains(
            "non-existent-spread",
            error.Error);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private sealed record ErrorResponse(string Error);
}
