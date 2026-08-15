namespace Tarot.Application.Interpretations;

public sealed class GetReadingInterpretationService
    : IGetReadingInterpretationService
{
    private readonly IReadingInterpretationRepository _repository;

    public GetReadingInterpretationService(
        IReadingInterpretationRepository repository)
    {
        _repository = repository;
    }

    public Task<ReadingInterpretationDetails?> GetByIdAsync(
        Guid interpretationId,
        CancellationToken cancellationToken = default)
    {
        return _repository.GetByIdAsync(
            interpretationId,
            cancellationToken);
    }

    public Task<ReadingInterpretationDetails?> GetByIdAsync(
        Guid readingId,
        Guid interpretationId,
        CancellationToken cancellationToken = default)
    {
        return _repository.GetByIdAsync(
            readingId,
            interpretationId,
            cancellationToken);
    }
}
