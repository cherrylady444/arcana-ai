using Tarot.Application.SpreadRecommendations;
using Tarot.Application.Spreads;
using Tarot.Tests.TestDoubles;

namespace Tarot.Tests.SpreadRecommendations;

public sealed class SpreadRecommendationServiceTests
{
    [Fact]
    public async Task RecommendAsync_LoadsBuiltInCatalogueAndReturnsKnownSpread()
    {
        var catalog = new BuiltInSpreadCatalog();
        var recommender = new FakeSpreadRecommender
        {
            RecommendationToReturn = new SpreadRecommendationDecision(
                RecommendationType: "existing",
                Reason: "  It offers enough structure for a nuanced decision.  ",
                SpreadKey: "five-card",
                CustomSpread: null)
        };

        var service = new SpreadRecommendationService(catalog, recommender);

        var result = await service.RecommendAsync(
            "  I am weighing a change and want to understand what is blocking me.  ");

        Assert.Equal("existing", result.RecommendationType);
        Assert.False(result.Spread.IsCustom);
        Assert.Equal("five-card", result.Spread.Key);
        Assert.Equal(
            "It offers enough structure for a nuanced decision.",
            result.Reason);

        var input = Assert.IsType<SpreadRecommendationInput>(recommender.LastInput);
        Assert.Equal(
            "I am weighing a change and want to understand what is blocking me.",
            input.Situation);
        Assert.Equal(catalog.GetBuiltInSpreads().Count, input.Spreads.Count);
    }

    [Fact]
    public async Task RecommendAsync_WhenProviderReturnsUnknownExistingSpread_Throws()
    {
        var recommender = new FakeSpreadRecommender
        {
            RecommendationToReturn = new SpreadRecommendationDecision(
                RecommendationType: "existing",
                Reason: "A made-up spread should never be accepted.",
                SpreadKey: "invented-spread",
                CustomSpread: null)
        };

        var service = new SpreadRecommendationService(
            new BuiltInSpreadCatalog(),
            recommender);

        await Assert.ThrowsAsync<SpreadRecommenderResponseException>(
            () => service.RecommendAsync(
                "I want perspective on a complicated situation."));
    }

    [Fact]
    public async Task RecommendAsync_WhenProviderReturnsValidCustomSpread_NormalizesIt()
    {
        var recommender = new FakeSpreadRecommender
        {
            RecommendationToReturn = new SpreadRecommendationDecision(
                RecommendationType: "custom",
                Reason: "This situation benefits from a tailored decision structure.",
                SpreadKey: null,
                CustomSpread: new GeneratedSpread(
                    Name: "Threshold of Change",
                    Description: "A focused spread for examining a major transition.",
                    Positions:
                    [
                        new GeneratedSpreadPosition(1, "Pull Forward", "What is drawing me toward this change?", "The motivation behind moving forward."),
                        new GeneratedSpreadPosition(2, "Hidden Cost", "What risk or cost am I underestimating?", "A blind spot or tradeoff to examine."),
                        new GeneratedSpreadPosition(3, "Guiding Principle", "What should guide my decision?", "The perspective that deserves priority.")
                    ]))
        };

        var service = new SpreadRecommendationService(
            new BuiltInSpreadCatalog(),
            recommender);

        var result = await service.RecommendAsync(
            "I am considering a move that affects work and relationships.");

        Assert.Equal("custom", result.RecommendationType);
        Assert.True(result.Spread.IsCustom);
        Assert.Null(result.Spread.Key);
        Assert.Equal("Threshold of Change", result.Spread.Name);
        Assert.Equal(3, result.Spread.CardCount);
        Assert.Equal(
            new[] { "position-1", "position-2", "position-3" },
            result.Spread.Positions.Select(position => position.Key));
    }

    [Fact]
    public async Task RecommendAsync_WhenCustomSpreadExceedsMaximumPositions_Throws()
    {
        var positions = Enumerable.Range(1, 11)
            .Select(order => new GeneratedSpreadPosition(
                order,
                $"Position {order}",
                $"What does position {order} explore?",
                $"Description for position {order}."))
            .ToArray();

        var recommender = new FakeSpreadRecommender
        {
            RecommendationToReturn = new SpreadRecommendationDecision(
                RecommendationType: "custom",
                Reason: "Too many positions.",
                SpreadKey: null,
                CustomSpread: new GeneratedSpread(
                    "Oversized Spread",
                    "This should be rejected.",
                    positions))
        };

        var service = new SpreadRecommendationService(
            new BuiltInSpreadCatalog(),
            recommender);

        await Assert.ThrowsAsync<SpreadRecommenderResponseException>(
            () => service.RecommendAsync("A complex situation."));
    }
}
