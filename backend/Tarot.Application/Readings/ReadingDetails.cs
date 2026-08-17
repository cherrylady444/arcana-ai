namespace Tarot.Application.Readings;

public sealed record ReadingDetails(
    Guid Id,
    string? UserContext,
    string SpreadKey,
    string SpreadName,
    IReadOnlyCollection<ReadingCardDetails> Cards,
    DateTimeOffset CreatedAtUtc,
    string SpreadDescription = "",
    bool IsCustomSpread = false
);

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
