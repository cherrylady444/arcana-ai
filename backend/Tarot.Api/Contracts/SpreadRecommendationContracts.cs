namespace Tarot.Api.Contracts;

public sealed record SpreadRecommendationRequest(
    string Situation
);

public sealed record SpreadRecommendationResponse(
    string RecommendationType,
    string Reason,
    RecommendedSpreadResponse Spread
);

public sealed record RecommendedSpreadResponse(
    string? Key,
    string Name,
    string Description,
    bool IsCustom,
    int CardCount,
    IReadOnlyCollection<RecommendedSpreadPositionResponse> Positions
);

public sealed record RecommendedSpreadPositionResponse(
    int Order,
    string Key,
    string Title,
    string Prompt,
    string Description
);
