namespace Tarot.Application.Interpretations;

/// <summary>
/// Retrieves one persisted interpretation either as an independent resource
/// or constrained to a specific reading.
/// </summary>
public interface IGetReadingInterpretationService
{
    Task<ReadingInterpretationDetails?> GetByIdAsync(
        Guid interpretationId,
        CancellationToken cancellationToken = default);

    Task<ReadingInterpretationDetails?> GetByIdAsync(
        Guid readingId,
        Guid interpretationId,
        CancellationToken cancellationToken = default);
}
