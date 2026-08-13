namespace Tarot.Infrastructure.Persistence.Entities;

public sealed class ReadingPositionInterpretationEntity
{
    public Guid Id { get; set; }

    public Guid ReadingInterpretationId { get; set; }

    public ReadingInterpretationEntity ReadingInterpretation
    {
        get;
        set;
    } = null!;

    public int PositionOrder { get; set; }

    public string Interpretation { get; set; } = string.Empty;
}