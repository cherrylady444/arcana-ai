namespace Tarot.Infrastructure.Persistence.Entities;

public sealed class ReadingEntity
{
    public Guid Id { get; set; }

    public string? UserContext { get; set; }

    public string SpreadKey { get; set; } = string.Empty;
    public string SpreadName { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public List<ReadingCardEntity> Cards { get; set; } = new();
}