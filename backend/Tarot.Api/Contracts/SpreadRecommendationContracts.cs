namespace Tarot.Api.Contracts;

public sealed record SpreadRecommendationRequest(
    string Situation
);

public sealed record SpreadRecommendationResponse(
    string SpreadKey,
    string Reason
);
