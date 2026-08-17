using Tarot.Application.SpreadRecommendations;

namespace Tarot.Tests.TestDoubles;

public sealed class FakeSpreadRecommender : ISpreadRecommender
{
    public SpreadRecommendationDecision RecommendationToReturn { get; set; } =
        new(
            RecommendationType: "existing",
            Reason: "This spread gives the situation a balanced overview.",
            SpreadKey: "three-card",
            CustomSpread: null);

    public SpreadRecommendationInput? LastInput { get; private set; }
    public int CallCount { get; private set; }

    public Task<SpreadRecommendationDecision> RecommendAsync(
        SpreadRecommendationInput input,
        CancellationToken cancellationToken = default)
    {
        LastInput = input;
        CallCount++;

        return Task.FromResult(RecommendationToReturn);
    }
}
