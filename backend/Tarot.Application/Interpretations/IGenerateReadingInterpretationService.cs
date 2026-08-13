namespace Tarot.Application.Interpretations;

/// <summary>
/// Generates and persists an interpretation for an existing persisted reading.
/// </summary>
public interface IGenerateReadingInterpretationService
{
    /// <summary>
    /// Returns null when the requested reading does not exist.
    /// </summary>
    Task<ReadingInterpretationDetails?> GenerateAsync(
        Guid readingId,
        CancellationToken cancellationToken = default);
}