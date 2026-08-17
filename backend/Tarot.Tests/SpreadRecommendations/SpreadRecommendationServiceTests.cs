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
            RecommendationToReturn = new SpreadRecommendation(
                SpreadKey: "five-card",
                Reason: "  It offers enough structure for a nuanced decision.  ")
        };

        var service = new SpreadRecommendationService(
            catalog,
            recommender);

        var result = await service.RecommendAsync(
            "  I am weighing a change and want to understand what is blocking me.  ");

        Assert.Equal("five-card", result.SpreadKey);
        Assert.Equal(
            "It offers enough structure for a nuanced decision.",
            result.Reason);

        var input = Assert.IsType<SpreadRecommendationInput>(
            recommender.LastInput);

        Assert.Equal(
            "I am weighing a change and want to understand what is blocking me.",
            input.Situation);
        Assert.Equal(catalog.GetBuiltInSpreads().Count, input.Spreads.Count);
        Assert.Contains(input.Spreads, spread => spread.Key == "five-card");
    }

    [Fact]
    public async Task RecommendAsync_WhenProviderReturnsUnknownSpread_Throws()
    {
        var recommender = new FakeSpreadRecommender
        {
            RecommendationToReturn = new SpreadRecommendation(
                SpreadKey: "invented-spread",
                Reason: "A made-up spread should never be accepted.")
        };

        var service = new SpreadRecommendationService(
            new BuiltInSpreadCatalog(),
            recommender);

        await Assert.ThrowsAsync<SpreadRecommenderResponseException>(
            () => service.RecommendAsync(
                "I want perspective on a complicated situation."));
    }
}
