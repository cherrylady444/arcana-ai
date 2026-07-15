namespace Tarot.Domain;

public sealed class TarotCard
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Arcana { get; init; } = string.Empty;
    public string? Suit { get; init; }
    public string UprightMeaning { get; init; } = string.Empty;
    public string ReversedMeaning { get; init; } = string.Empty;
}
