namespace Tarot.Application.Interpretations;

public interface IListReadingInterpretationsService
{
    /// <summary>
    /// Returns null when the reading does not exist.
    /// Returns an empty collection when the reading exists but has no
    /// interpretations.
    /// </summary>
    Task<IReadOnlyCollection<ReadingInterpretationDetails>?>
        GetByReadingIdAsync(
            Guid readingId,
            CancellationToken cancellationToken = default);
}