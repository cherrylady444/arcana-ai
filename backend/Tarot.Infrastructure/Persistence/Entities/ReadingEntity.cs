namespace Tarot.Infrastructure.Persistence.Entities;

public sealed class ReadingEntity
{
    public Guid Id { get; set; }

    public string? UserContext { get; set; }

    public string SpreadKey { get; set; } = string.Empty;
    public string SpreadName { get; set; } = string.Empty;
    public string SpreadDescription { get; set; } = string.Empty;
    public bool IsCustomSpread { get; set; }

    public Guid? SourceConversationId { get; set; }
    public ConversationEntity? SourceConversation { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public List<ReadingCardEntity> Cards { get; set; } = new();

    public List<ReadingInterpretationEntity> Interpretations { get; set; }
        = new();

    public List<ConversationEntity> Conversations { get; set; } = [];
}
