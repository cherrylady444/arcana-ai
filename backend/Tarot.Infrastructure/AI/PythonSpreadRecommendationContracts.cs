namespace Tarot.Infrastructure.AI;

internal sealed record PythonSpreadRecommendationRequest(
    string? Situation,
    IReadOnlyCollection<PythonSpreadRecommendationMessage>? ConversationMessages,
    IReadOnlyCollection<PythonSpreadRecommendationCandidate> Spreads
);

internal sealed record PythonSpreadRecommendationMessage(
    string Role,
    string Content
);

internal sealed record PythonSpreadRecommendationCandidate(
    string Key,
    string Name,
    string ShortDescription,
    string Description,
    string Intent,
    IReadOnlyCollection<PythonSpreadRecommendationPosition> Positions
);

internal sealed record PythonSpreadRecommendationPosition(
    int Order,
    string Key,
    string Title,
    string Prompt,
    string Description
);

internal sealed record PythonSpreadRecommendationResponse(
    string? RecommendationType,
    string? Reason,
    string? SpreadKey,
    PythonGeneratedSpread? CustomSpread
);

internal sealed record PythonGeneratedSpread(
    string? Name,
    string? Description,
    IReadOnlyCollection<PythonGeneratedSpreadPosition>? Positions
);

internal sealed record PythonGeneratedSpreadPosition(
    int Order,
    string? Title,
    string? Prompt,
    string? Description
);
