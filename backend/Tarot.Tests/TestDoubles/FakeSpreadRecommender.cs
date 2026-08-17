using Tarot.Application.SpreadRecommendations;

namespace Tarot.Tests.TestDoubles;

public sealed class FakeSpreadRecommender : ISpreadRecommender
{
    public SpreadRecommendation RecommendationToReturn { get; set; } =
        new(
            SpreadKey: "three-card",
            Reason: "This spread gives the situation a balanced overview.");

    public SpreadRecommendationInput? LastInput { get; private set; }
    public int CallCount { get; private set; }

    public Task<SpreadRecommendation> RecommendAsync(
        SpreadRecommendationInput input,
        CancellationToken cancellationToken = default)
    {
        LastInput = input;
        CallCount++;

        return Task.FromResult(RecommendationToReturn);
    }
}
