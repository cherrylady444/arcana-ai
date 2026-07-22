namespace Tarot.Api.Contracts;

/// <summary>
/// Compact representation of a spread returned by GET /api/spreads.
/// </summary>
public sealed record SpreadSummaryResponse(
    string Key,
    string Name,
    string ShortDescription,
    string Intent,
    int CardCount
);

/// <summary>
/// Complete representation of one spread returned by
/// GET /api/spreads/{spreadKey}.
/// </summary>
public sealed record SpreadDetailResponse(
    string Key,
    string Name,
    string ShortDescription,
    string Description,
    string Intent,
    int CardCount,
    IReadOnlyCollection<SpreadPositionResponse> Positions
);

/// <summary>
/// Represents one ordered position defined by a tarot spread.
/// </summary>
public sealed record SpreadPositionResponse(
    int Order,
    string Key,
    string Title,
    string Prompt,
    string Description
);