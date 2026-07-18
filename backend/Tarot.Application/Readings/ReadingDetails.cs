namespace Tarot.Application.Readings;

/// <summary>
/// Represents a previously saved tarot reading.
/// This is a read model containing the exact snapshot stored in PostgreSQL.
/// </summary>
public sealed record ReadingDetails(
    Guid Id,
    string? UserContext,
    string SpreadKey,
    string SpreadName,
    IReadOnlyCollection<ReadingCardDetails> Cards,
    DateTimeOffset CreatedAtUtc
);

/// <summary>
/// Represents one saved card and its position inside a reading.
/// </summary>
public sealed record ReadingCardDetails(
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