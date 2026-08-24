namespace Tarot.Application.SpreadRecommendations;

public interface ISpreadRecommendationService
{
    Task<SpreadRecommendation> RecommendAsync(
        string situation,
        CancellationToken cancellationToken = default);

    Task<SpreadRecommendation> RecommendFromConversationAsync(
        IReadOnlyCollection<SpreadRecommendationMessage> messages,
        CancellationToken cancellationToken = default);
}
