namespace Tarot.Application.Interpretations;

/// <summary>
/// Get one persisted interpretation
/// </summary>
public interface IGetReadingInterpretationService
{
    Task<ReadingInterpretationDetails?> GetByIdAsync(
        Guid readingId,
        Guid interpretationId,
        CancellationToken cancellationToken = default);
}