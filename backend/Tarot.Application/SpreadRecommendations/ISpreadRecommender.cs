namespace Tarot.Application.SpreadRecommendations;

/// <summary>
/// Capability for choosing one spread from a trusted set of predefined spreads.
/// The Application layer does not know whether this is implemented by Python,
/// OpenAI, another provider, or a test fake.
/// </summary>
public interface ISpreadRecommender
{
    Task<SpreadRecommendation> RecommendAsync(
        SpreadRecommendationInput input,
        CancellationToken cancellationToken = default);
}
