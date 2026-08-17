using Tarot.Application.Spreads;

namespace Tarot.Application.SpreadRecommendations;

/// <summary>
/// Loads the authoritative built-in spread catalogue and asks the AI layer to
/// select one spread for the user's situation.
/// </summary>
public sealed class SpreadRecommendationService : ISpreadRecommendationService
{
    public const int MaxSituationLength = 1000;

    private readonly ISpreadCatalog _spreadCatalog;
    private readonly ISpreadRecommender _spreadRecommender;

    public SpreadRecommendationService(
        ISpreadCatalog spreadCatalog,
        ISpreadRecommender spreadRecommender)
    {
        _spreadCatalog = spreadCatalog;
        _spreadRecommender = spreadRecommender;
    }

    public async Task<SpreadRecommendation> RecommendAsync(
        string situation,
        CancellationToken cancellationToken = default)
    {
        var normalizedSituation = situation?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedSituation))
        {
            throw new ArgumentException(
                "A situation or question is required.",
                nameof(situation));
        }

        if (normalizedSituation.Length > MaxSituationLength)
        {
            throw new ArgumentException(
                $"The situation cannot exceed {MaxSituationLength} characters.",
                nameof(situation));
        }

        var spreads = _spreadCatalog.GetBuiltInSpreads();

        if (spreads.Count == 0)
        {
            throw new InvalidOperationException(
                "No predefined spreads are available for recommendation.");
        }

        var candidates = spreads
            .Select(spread => new SpreadRecommendationCandidate(
                Key: spread.Key,
                Name: spread.Name,
                ShortDescription: spread.ShortDescription,
                Description: spread.Description,
                Intent: spread.Intent,
                Positions: spread.Positions
                    .OrderBy(position => position.Order)
                    .Select(position => new SpreadRecommendationPosition(
                        Order: position.Order,
                        Key: position.Key,
                        Title: position.Title,
                        Prompt: position.Prompt,
                        Description: position.Description))
                    .ToArray()))
            .ToArray();

        var recommendation = await _spreadRecommender.RecommendAsync(
            new SpreadRecommendationInput(
                Situation: normalizedSituation,
                Spreads: candidates),
            cancellationToken);

        var recommendedSpread = _spreadCatalog.GetByKey(
            recommendation.SpreadKey);

        if (recommendedSpread is null)
        {
            throw new SpreadRecommenderResponseException(
                "The AI service recommended a spread that is not in the predefined catalogue.");
        }

        if (string.IsNullOrWhiteSpace(recommendation.Reason))
        {
            throw new SpreadRecommenderResponseException(
                "The AI service returned an empty recommendation reason.");
        }

        return new SpreadRecommendation(
            SpreadKey: recommendedSpread.Key,
            Reason: recommendation.Reason.Trim());
    }
}
