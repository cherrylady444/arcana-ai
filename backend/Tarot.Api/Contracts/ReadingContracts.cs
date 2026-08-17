namespace Tarot.Api.Contracts;

/// <summary>
/// Request body accepted by POST /api/readings.
/// Exactly one of spreadKey/customSpread may be supplied. If both are omitted,
/// the existing default spread behavior is preserved.
/// </summary>
public sealed record CreateReadingRequest(
    string? SpreadKey,
    string? UserContext,
    CustomSpreadRequest? CustomSpread = null
);

public sealed record CustomSpreadRequest(
    string Name,
    string Description,
    IReadOnlyCollection<CustomSpreadPositionRequest> Positions
);

public sealed record CustomSpreadPositionRequest(
    int Order,
    string Title,
    string Prompt,
    string Description
);

/// <summary>
/// Complete reading returned after creating a reading or retrieving one by ID.
/// </summary>
public sealed record ReadingResponse(
    Guid Id,
    string? UserContext,
    string SpreadKey,
    string SpreadName,
    string SpreadDescription,
    bool IsCustomSpread,
    IReadOnlyCollection<ReadingCardResponse> Cards,
    DateTimeOffset CreatedAtUtc
);

public sealed record ReadingCardResponse(
    int PositionOrder,
    string PositionKey,
    string PositionTitle,
    string PositionPrompt,
    string PositionDescription,
    Guid CardId,
    string CardName,
    string Arcana,
    string? Suit,
    string Orientation,
    string Meaning
);
