using Tarot.Application.Interpretations;

namespace Tarot.Tests.TestDoubles;

/// <summary>
/// In-memory test implementation of IReadingInterpretationRepository.
///
/// Stores generated interpretations without PostgreSQL so unit tests can verify
/// whether application services persisted an interpretation.
/// </summary>
internal sealed class FakeReadingInterpretationRepository
    : IReadingInterpretationRepository
{
    public List<ReadingInterpretationDetails> SavedInterpretations { get; } = [];

    public Task SaveAsync(
        ReadingInterpretationDetails interpretation,
        CancellationToken cancellationToken = default)
    {
        SavedInterpretations.Add(interpretation);

        return Task.CompletedTask;
    }

    public Task<ReadingInterpretationDetails?> GetByIdAsync(
        Guid interpretationId,
        CancellationToken cancellationToken = default)
    {
        var result = SavedInterpretations.SingleOrDefault(
            interpretation =>
                interpretation.Id == interpretationId);

        return Task.FromResult(result);
    }

    public Task<ReadingInterpretationDetails?> GetByIdAsync(
        Guid readingId,
        Guid interpretationId,
        CancellationToken cancellationToken = default)
    {
        var result = SavedInterpretations.SingleOrDefault(
            interpretation =>
                interpretation.ReadingId == readingId &&
                interpretation.Id == interpretationId);

        return Task.FromResult(result);
    }

    public Task<IReadOnlyCollection<ReadingInterpretationDetails>>
        GetByReadingIdAsync(
            Guid readingId,
            CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<ReadingInterpretationDetails> result =
            SavedInterpretations
                .Where(interpretation =>
                    interpretation.ReadingId == readingId)
                .OrderByDescending(interpretation =>
                    interpretation.CreatedAtUtc)
                .ToArray();

        return Task.FromResult(result);
    }
}