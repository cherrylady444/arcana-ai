namespace Tarot.Infrastructure.Persistence.Entities;

public sealed class ReadingInterpretationReflectionQuestionEntity
{
    public Guid Id { get; set; }

    public Guid ReadingInterpretationId { get; set; }

    public ReadingInterpretationEntity ReadingInterpretation
    {
        get;
        set;
    } = null!;

    public int QuestionOrder { get; set; }

    public string Question { get; set; } = string.Empty;
}