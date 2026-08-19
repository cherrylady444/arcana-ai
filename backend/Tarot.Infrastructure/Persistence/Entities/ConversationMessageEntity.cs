namespace Tarot.Infrastructure.Persistence.Entities;

public sealed class ConversationMessageEntity
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }
    public ConversationEntity Conversation { get; set; } = null!;

    public int Sequence { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
}
