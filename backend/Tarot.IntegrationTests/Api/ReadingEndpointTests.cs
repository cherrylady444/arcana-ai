using System.Net;
using System.Net.Http.Json;
using Tarot.Api.Contracts;
using Tarot.Application.Readings;
using Tarot.IntegrationTests.Infrastructure;
using Tarot.Application.Interpretations;

namespace Tarot.IntegrationTests.Api;

/// <summary>
/// Exercises reading creation, persistence, retrieval, and pagination through
/// HttpClient -> ASP.NET Core -> Application -> EF Core -> PostgreSQL.
/// </summary>
public sealed class ReadingEndpointTests :
    IClassFixture<PostgreSqlFixture>,
    IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;
    private readonly TarotApiFactory _factory;
    private readonly HttpClient _client;

    public ReadingEndpointTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
        _factory = new TarotApiFactory(fixture.ConnectionString);
        _client = _factory.CreateClient();
    }

    /// <summary>
    /// xUnit runs this before every test method. Each test therefore starts
    /// with the real migrated PostgreSQL schema and no reading data.
    /// </summary>
    public Task InitializeAsync()
    {
        return _fixture.RecreateDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();

        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateReading_ThenGetById_ReturnsPersistedReading()
    {
        var spread = await GetSpreadAsync("five-card");
        const string userContext =
            "What should I focus on this week?";

        var createResponse = await _client.PostAsJsonAsync(
            "/api/readings",
            new CreateReadingRequest(
                SpreadKey: spread.Key,
                UserContext: userContext));

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

        var created = await createResponse.Content
            .ReadFromJsonAsync<ReadingResponse>();

        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(spread.Key, created.SpreadKey);
        Assert.Equal(spread.Name, created.SpreadName);
        Assert.Equal(userContext, created.UserContext);
        Assert.Equal(spread.CardCount, created.Cards.Count);

        var spreadPositions = spread.Positions
            .OrderBy(position => position.Order)
            .ToArray();

        var createdCards = created.Cards.ToArray();

        Assert.Equal(
            spreadPositions.Select(position => position.Order),
            createdCards.Select(card => card.PositionOrder));

        Assert.Equal(
            spreadPositions.Select(position => position.Key),
            createdCards.Select(card => card.PositionKey));

        Assert.Equal(
            createdCards.Length,
            createdCards.Select(card => card.CardId).Distinct().Count());

        Assert.All(createdCards, card =>
        {
            Assert.NotEqual(Guid.Empty, card.CardId);
            Assert.False(string.IsNullOrWhiteSpace(card.CardName));
            Assert.False(string.IsNullOrWhiteSpace(card.Arcana));
            Assert.False(string.IsNullOrWhiteSpace(card.PositionTitle));
            Assert.False(string.IsNullOrWhiteSpace(card.PositionPrompt));
            Assert.False(string.IsNullOrWhiteSpace(card.PositionDescription));
            Assert.False(string.IsNullOrWhiteSpace(card.Meaning));
            Assert.Contains(
                card.Orientation,
                new[] { "Upright", "Reversed" });
        });

        var getResponse = await _client.GetAsync(
            $"/api/readings/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var retrieved = await getResponse.Content
            .ReadFromJsonAsync<ReadingResponse>();

        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
        Assert.Equal(created.SpreadKey, retrieved.SpreadKey);
        Assert.Equal(created.SpreadName, retrieved.SpreadName);
        Assert.Equal(created.UserContext, retrieved.UserContext);
        Assert.True(
            created.Cards.SequenceEqual(retrieved.Cards));

        /*
         * PostgreSQL timestamp with time zone is stored with microsecond
         * precision, while DateTimeOffset can represent finer 100 ns ticks.
         * The persisted timestamp should therefore be effectively identical,
         * allowing only sub-millisecond storage precision loss.
         */
        Assert.True(
            (retrieved.CreatedAtUtc - created.CreatedAtUtc).Duration()
                < TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task CreateReading_WithUnknownSpread_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/readings",
            new CreateReadingRequest(
                SpreadKey: "non-existent-spread",
                UserContext: "This should not be persisted."));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content
            .ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(error);
        Assert.Contains(
            "Unknown spread key: non-existent-spread",
            error.Error);
    }

    [Fact]
    public async Task GetReadingById_WhenReadingDoesNotExist_ReturnsNotFound()
    {
        var readingId = Guid.NewGuid();

        var response = await _client.GetAsync(
            $"/api/readings/{readingId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var error = await response.Content
            .ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(error);
        Assert.Contains(
            readingId.ToString(),
            error.Error);
    }

    [Fact]
    public async Task ListReadings_ReturnsRequestedPageNewestFirstWithoutDuplicates()
    {
        var createdIds = new HashSet<Guid>();

        for (var index = 1; index <= 5; index++)
        {
            var reading = await CreateReadingAsync(
                spreadKey: "one-card",
                userContext: $"API pagination reading {index}");

            createdIds.Add(reading.Id);
        }

        var firstPageResponse = await _client.GetAsync(
            "/api/readings?page=1&pageSize=3");

        Assert.Equal(HttpStatusCode.OK, firstPageResponse.StatusCode);

        var firstPage = await firstPageResponse.Content
            .ReadFromJsonAsync<PagedReadingsResult>();

        Assert.NotNull(firstPage);
        Assert.Equal(1, firstPage.Page);
        Assert.Equal(3, firstPage.PageSize);
        Assert.Equal(5, firstPage.TotalCount);
        Assert.Equal(2, firstPage.TotalPages);
        Assert.Equal(3, firstPage.Items.Count);

        var firstPageIds = firstPage.Items
            .Select(reading => reading.Id)
            .ToArray();

        Assert.Equal(firstPageIds.Length, firstPageIds.Distinct().Count());
        Assert.All(firstPageIds, id => Assert.Contains(id, createdIds));

        var expectedFirstPageOrder = firstPage.Items
            .OrderByDescending(reading => reading.CreatedAtUtc)
            .ThenByDescending(reading => reading.Id)
            .Select(reading => reading.Id)
            .ToArray();

        Assert.Equal(expectedFirstPageOrder, firstPageIds);

        var secondPageResponse = await _client.GetAsync(
            "/api/readings?page=2&pageSize=3");

        Assert.Equal(HttpStatusCode.OK, secondPageResponse.StatusCode);

        var secondPage = await secondPageResponse.Content
            .ReadFromJsonAsync<PagedReadingsResult>();

        Assert.NotNull(secondPage);
        Assert.Equal(2, secondPage.Page);
        Assert.Equal(3, secondPage.PageSize);
        Assert.Equal(5, secondPage.TotalCount);
        Assert.Equal(2, secondPage.TotalPages);
        Assert.Equal(2, secondPage.Items.Count);

        var secondPageIds = secondPage.Items
            .Select(reading => reading.Id)
            .ToArray();

        Assert.Empty(firstPageIds.Intersect(secondPageIds));
        Assert.All(secondPageIds, id => Assert.Contains(id, createdIds));
    }

    [Fact]
    public async Task ListReadings_WithPageZero_ReturnsBadRequest()
    {
        var response = await _client.GetAsync(
            "/api/readings?page=0&pageSize=20");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content
            .ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(error);
        Assert.Contains(
            "Page must be at least 1.",
            error.Error);
    }

    [Fact]
    public async Task GenerateInterpretation_LoadsPersistedReadingSnapshot()
    {
        const string userContext =
            "Should I change direction in my career?";

        var created = await CreateReadingAsync(
            spreadKey: "one-card",
            userContext: userContext);

        var response = await _client.PostAsync(
            $"/api/readings/{created.Id}/interpretations",
            content: null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var interpretation = await response.Content
            .ReadFromJsonAsync<ReadingInterpretationResponse>();

        Assert.NotNull(interpretation);

        Assert.Equal(
            $"/api/readings/{created.Id}/interpretations/{interpretation.Id}",
            response.Headers.Location!.OriginalString);

        Assert.NotEqual(Guid.Empty, interpretation.Id);
        Assert.Equal(created.Id, interpretation.ReadingId);

        Assert.False(string.IsNullOrWhiteSpace(
            interpretation.OverallTheme));
        Assert.Single(interpretation.PositionInterpretations);
        Assert.Equal(2, interpretation.ReflectionQuestions.Count);

        Assert.Equal(1, _factory.ReadingInterpreter.CallCount);

        var input = Assert.IsType<ReadingInterpretationInput>(
            _factory.ReadingInterpreter.LastInput);

        Assert.Equal(created.Id, input.ReadingId);
        Assert.Equal(created.UserContext, input.UserContext);
        Assert.Equal(created.SpreadKey, input.Spread.Key);
        Assert.Equal(created.SpreadName, input.Spread.Name);

        var createdCard = Assert.Single(created.Cards);
        var placement = Assert.Single(input.Placements);

        Assert.Equal(createdCard.PositionOrder, placement.PositionOrder);
        Assert.Equal(createdCard.PositionKey, placement.PositionKey);
        Assert.Equal(createdCard.PositionTitle, placement.PositionTitle);
        Assert.Equal(createdCard.PositionPrompt, placement.PositionPrompt);
        Assert.Equal(
            createdCard.PositionDescription,
            placement.PositionDescription);
        Assert.Equal(createdCard.CardId, placement.CardId);
        Assert.Equal(createdCard.CardName, placement.CardName);
        Assert.Equal(createdCard.Orientation, placement.Orientation);
        Assert.Equal(createdCard.Meaning, placement.Meaning);
    }

    [Fact]
    public async Task GetInterpretationById_AfterGeneration_ReturnsPersistedInterpretation()
    {
        var created = await CreateReadingAsync(
            spreadKey: "one-card",
            userContext: "What should I understand right now?");

        var generateResponse = await _client.PostAsync(
            $"/api/readings/{created.Id}/interpretations",
            content: null);

        Assert.Equal(HttpStatusCode.Created, generateResponse.StatusCode);

        var generated = await generateResponse.Content
            .ReadFromJsonAsync<ReadingInterpretationResponse>();

        Assert.NotNull(generated);

        var response = await _client.GetAsync(
            $"/api/interpretations/{generated.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var retrieved = await response.Content
            .ReadFromJsonAsync<ReadingInterpretationResponse>();

        Assert.NotNull(retrieved);
        Assert.Equal(generated.Id, retrieved.Id);
        Assert.Equal(created.Id, retrieved.ReadingId);
        Assert.Equal(generated.OverallTheme, retrieved.OverallTheme);
        Assert.Equal(generated.Synthesis, retrieved.Synthesis);
        Assert.Equal(
            generated.PositionInterpretations,
            retrieved.PositionInterpretations);
        Assert.Equal(
            generated.ReflectionQuestions,
            retrieved.ReflectionQuestions);
    }

    [Fact]
    public async Task GetInterpretationById_WhenMissing_ReturnsNotFound()
    {
        var interpretationId = Guid.NewGuid();

        var response = await _client.GetAsync(
            $"/api/interpretations/{interpretationId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var error = await response.Content
            .ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(error);
        Assert.Contains(interpretationId.ToString(), error.Error);
    }

    [Fact]
    public async Task GenerateInterpretation_WhenReadingDoesNotExist_ReturnsNotFound()
    {
        var readingId = Guid.NewGuid();

        var response = await _client.PostAsync(
            $"/api/readings/{readingId}/interpretations",
            content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(0, _factory.ReadingInterpreter.CallCount);

        var error = await response.Content
            .ReadFromJsonAsync<ErrorResponse>();

        Assert.NotNull(error);
        Assert.Contains(readingId.ToString(), error.Error);
    }


    private async Task<SpreadDetailResponse> GetSpreadAsync(
        string spreadKey)
    {
        var response = await _client.GetAsync(
            $"/api/spreads/{spreadKey}");

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<SpreadDetailResponse>()
            ?? throw new InvalidOperationException(
                $"Spread '{spreadKey}' returned an empty response body.");
    }

    private async Task<ReadingResponse> CreateReadingAsync(
        string spreadKey,
        string userContext)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/readings",
            new CreateReadingRequest(
                SpreadKey: spreadKey,
                UserContext: userContext));

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<ReadingResponse>()
            ?? throw new InvalidOperationException(
                "POST /api/readings returned an empty response body.");
    }

    private sealed record ErrorResponse(string Error);
}
