namespace Tarot.Application.Interpretations;

/// <summary>
/// Persists and retrieves generated reading interpretations.
///
/// PostgreSQL implements this abstraction in Tarot.Infrastructure.
/// </summary>
public interface IReadingInterpretationRepository
{
    Task SaveAsync(
        ReadingInterpretationDetails interpretation,
        CancellationToken cancellationToken = default);

    Task<ReadingInterpretationDetails?> GetByIdAsync(
        Guid readingId,
        Guid interpretationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ReadingInterpretationDetails>>
        GetByReadingIdAsync(
            Guid readingId,
            CancellationToken cancellationToken = default);
}