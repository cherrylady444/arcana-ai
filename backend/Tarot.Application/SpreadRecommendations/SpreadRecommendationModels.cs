namespace Tarot.Application.SpreadRecommendations;

/// <summary>
/// User situation plus the trusted built-in spreads that the AI may compare
/// against. ASP.NET constructs this input; the browser never supplies
/// authoritative built-in spread definitions.
/// </summary>
public sealed record SpreadRecommendationInput(
    string Situation,
    IReadOnlyCollection<SpreadRecommendationCandidate> Spreads
);

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
/// Raw structured decision returned by the AI provider. The Application layer
/// still validates this result before exposing it to the browser.
/// </summary>
public sealed record SpreadRecommendationDecision(
    string RecommendationType,
    string Reason,
    string? SpreadKey,
    GeneratedSpread? CustomSpread
);

public sealed record GeneratedSpread(
    string Name,
    string Description,
    IReadOnlyCollection<GeneratedSpreadPosition> Positions
);

public sealed record GeneratedSpreadPosition(
    int Order,
    string Title,
    string Prompt,
    string Description
);

/// <summary>
/// Safe, normalized recommendation returned by the ASP.NET application.
/// Existing spreads are reloaded from the trusted catalogue. Custom spreads
/// are validated and normalized before they reach the frontend.
/// </summary>
public sealed record SpreadRecommendation(
    string RecommendationType,
    string Reason,
    RecommendedSpread Spread
);

public sealed record RecommendedSpread(
    string? Key,
    string Name,
    string Description,
    bool IsCustom,
    IReadOnlyCollection<SpreadRecommendationPosition> Positions
)
{
    public int CardCount => Positions.Count;
}
