using Tarot.Application.Spreads;

namespace Tarot.Application.SpreadRecommendations;

/// <summary>
/// Loads the authoritative built-in spread catalogue and asks the AI layer to
/// either select one existing spread or propose a bounded custom spread.
/// ASP.NET validates and normalizes either outcome before returning it.
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

        var decision = await _spreadRecommender.RecommendAsync(
            new SpreadRecommendationInput(
                Situation: normalizedSituation,
                Spreads: candidates),
            cancellationToken);

        if (string.IsNullOrWhiteSpace(decision.RecommendationType))
        {
            throw new SpreadRecommenderResponseException(
                "The AI service returned an empty recommendation type.");
        }

        if (string.IsNullOrWhiteSpace(decision.Reason))
        {
            throw new SpreadRecommenderResponseException(
                "The AI service returned an empty recommendation reason.");
        }

        return decision.RecommendationType.Trim().ToLowerInvariant() switch
        {
            "existing" => NormalizeExistingRecommendation(decision),
            "custom" => NormalizeCustomRecommendation(decision),
            _ => throw new SpreadRecommenderResponseException(
                "The AI service returned an unknown recommendation type.")
        };
    }

    private SpreadRecommendation NormalizeExistingRecommendation(
        SpreadRecommendationDecision decision)
    {
        if (string.IsNullOrWhiteSpace(decision.SpreadKey)
            || decision.CustomSpread is not null)
        {
            throw new SpreadRecommenderResponseException(
                "The AI service returned an invalid existing-spread recommendation.");
        }

        var spread = _spreadCatalog.GetByKey(decision.SpreadKey);

        if (spread is null)
        {
            throw new SpreadRecommenderResponseException(
                "The AI service recommended a spread that is not in the predefined catalogue.");
        }

        return new SpreadRecommendation(
            RecommendationType: "existing",
            Reason: decision.Reason.Trim(),
            Spread: new RecommendedSpread(
                Key: spread.Key,
                Name: spread.Name,
                Description: spread.Description,
                IsCustom: false,
                Positions: spread.Positions
                    .OrderBy(position => position.Order)
                    .Select(position => new SpreadRecommendationPosition(
                        position.Order,
                        position.Key,
                        position.Title,
                        position.Prompt,
                        position.Description))
                    .ToArray()));
    }

    private static SpreadRecommendation NormalizeCustomRecommendation(
        SpreadRecommendationDecision decision)
    {
        if (!string.IsNullOrWhiteSpace(decision.SpreadKey)
            || decision.CustomSpread is null)
        {
            throw new SpreadRecommenderResponseException(
                "The AI service returned an invalid custom-spread recommendation.");
        }

        try
        {
            var validated = CustomSpreadValidator.ValidateAndCreate(
                new CustomSpreadDefinition(
                    Name: decision.CustomSpread.Name,
                    Description: decision.CustomSpread.Description,
                    Positions: decision.CustomSpread.Positions
                        .Select(position => new CustomSpreadPositionDefinition(
                            position.Order,
                            position.Title,
                            position.Prompt,
                            position.Description))
                        .ToArray()),
                key: "custom-preview");

            return new SpreadRecommendation(
                RecommendationType: "custom",
                Reason: decision.Reason.Trim(),
                Spread: new RecommendedSpread(
                    Key: null,
                    Name: validated.Name,
                    Description: validated.Description,
                    IsCustom: true,
                    Positions: validated.Positions
                        .OrderBy(position => position.Order)
                        .Select(position => new SpreadRecommendationPosition(
                            position.Order,
                            position.Key,
                            position.Title,
                            position.Prompt,
                            position.Description))
                        .ToArray()));
        }
        catch (ArgumentException exception)
        {
            throw new SpreadRecommenderResponseException(
                "The AI service returned an invalid custom spread definition.",
                exception);
        }
    }
}
