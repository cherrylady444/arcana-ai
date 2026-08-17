namespace Tarot.Application.SpreadRecommendations;

/// <summary>
/// User situation plus the trusted built-in spreads that the AI may choose from.
/// ASP.NET constructs this input; the browser never supplies authoritative
/// spread definitions.
/// </summary>
public sealed record SpreadRecommendationInput(
    string Situation,
    IReadOnlyCollection<SpreadRecommendationCandidate> Spreads
);

/// <summary>
/// One predefined spread that is eligible for recommendation.
/// </summary>
public sealed record SpreadRecommendationCandidate(
    string Key,
    string Name,
    string ShortDescription,
    string Description,
    string Intent,
    IReadOnlyCollection<SpreadRecommendationPosition> Positions
);

public sealed record SpreadRecommendationPosition(
    int Order,
    string Key,
    string Title,
    string Prompt,
    string Description
);

/// <summary>
/// Structured recommendation returned by the AI layer.
/// </summary>
public sealed record SpreadRecommendation(
    string SpreadKey,
    string Reason
);
