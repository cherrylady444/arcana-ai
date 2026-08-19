namespace Tarot.Infrastructure.Persistence.Entities;

public sealed class ConversationEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;

    public Guid? ReadingId { get; set; }
    public ReadingEntity? Reading { get; set; }

    public Guid? InterpretationId { get; set; }
    public ReadingInterpretationEntity? Interpretation { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    public List<ConversationMessageEntity> Messages { get; set; } = [];
}
