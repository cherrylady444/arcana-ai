namespace Tarot.Application.SpreadRecommendations;

/// <summary>
/// Capability for choosing an existing spread or proposing a custom spread.
/// The Application layer does not know whether this is implemented by Python,
/// OpenAI, another provider, or a test fake.
/// </summary>
public interface ISpreadRecommender
{
    Task<SpreadRecommendationDecision> RecommendAsync(
        SpreadRecommendationInput input,
        CancellationToken cancellationToken = default);
}
