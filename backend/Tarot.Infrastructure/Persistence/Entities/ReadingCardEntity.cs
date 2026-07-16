namespace Tarot.Infrastructure.Persistence.Entities;

public sealed class ReadingCardEntity
{
    public Guid Id { get; set; }

    public Guid ReadingId { get; set; }
    public ReadingEntity Reading { get; set; } = null!;

    public int PositionOrder { get; set; }
    public string PositionKey { get; set; } = string.Empty;
    public string PositionTitle { get; set; } = string.Empty;
    public string PositionPrompt { get; set; } = string.Empty;
    public string PositionDescription { get; set; } = string.Empty;

    public Guid CardId { get; set; }
    public string CardName { get; set; } = string.Empty;
    public string Arcana { get; set; } = string.Empty;
    public string? Suit { get; set; }

    public string Orientation { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
}