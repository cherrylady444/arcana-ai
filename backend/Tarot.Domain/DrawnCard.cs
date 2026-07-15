namespace Tarot.Domain;

public sealed record DrawnCard(
    TarotCard Card,
    CardOrientation Orientation
);
