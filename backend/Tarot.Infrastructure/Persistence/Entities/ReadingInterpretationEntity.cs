namespace Tarot.Infrastructure.Persistence.Entities;

public sealed class ReadingInterpretationEntity
{
    public Guid Id { get; set; }

    public Guid ReadingId { get; set; }
    public ReadingEntity Reading { get; set; } = null!;

    public string OverallTheme { get; set; } = string.Empty;
    public string Synthesis { get; set; } = string.Empty;

    public Guid? SourceConversationId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public List<ReadingPositionInterpretationEntity>
        PositionInterpretations { get; set; } = [];

    public List<ReadingInterpretationReflectionQuestionEntity>
        ReflectionQuestions { get; set; } = [];

    public List<ConversationEntity> Conversations { get; set; } = [];
}