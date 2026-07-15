namespace Tarot.Domain;

public sealed record ReadingCard(
    TarotCard Card,
    CardOrientation Orientation,
    SpreadPositionDefinition Position,
    string Meaning
);
