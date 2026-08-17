namespace Tarot.Application.SpreadRecommendations;

public interface ISpreadRecommendationService
{
    Task<SpreadRecommendation> RecommendAsync(
        string situation,
        CancellationToken cancellationToken = default);
}
