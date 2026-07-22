namespace Tarot.Api.Contracts;

/// <summary>
/// Request body accepted by POST /api/readings.
/// </summary>
public sealed record CreateReadingRequest(
    string? SpreadKey,
    string? UserContext
);

/// <summary>
/// Complete reading returned after creating a reading
/// or retrieving an existing reading by ID.
/// </summary>
public sealed record ReadingResponse(
    Guid Id,
    string? UserContext,
    string SpreadKey,
    string SpreadName,
    IReadOnlyCollection<ReadingCardResponse> Cards,
    DateTimeOffset CreatedAtUtc
);

/// <summary>
/// Represents one selected tarot card together with the spread position
/// assigned to it.
/// </summary>
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