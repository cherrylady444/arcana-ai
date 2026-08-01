namespace Tarot.Application.Interpretations;

/// <summary>
/// Generates an interpretation for an existing persisted reading.
/// </summary>
public interface IGenerateReadingInterpretationService
{
    /// <summary>
    /// Returns null when the requested reading does not exist.
    /// </summary>
    Task<ReadingInterpretation?> GenerateAsync(
        Guid readingId,
        CancellationToken cancellationToken = default);
}