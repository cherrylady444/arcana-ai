using Tarot.Application.Readings;

namespace Tarot.Application.Interpretations;

public sealed class ListReadingInterpretationsService
    : IListReadingInterpretationsService
{
    private readonly IReadingRepository _readingRepository;
    private readonly IReadingInterpretationRepository
        _interpretationRepository;

    public ListReadingInterpretationsService(
        IReadingRepository readingRepository,
        IReadingInterpretationRepository interpretationRepository)
    {
        _readingRepository = readingRepository;
        _interpretationRepository = interpretationRepository;
    }

    public async Task<IReadOnlyCollection<ReadingInterpretationDetails>?>
        GetByReadingIdAsync(
            Guid readingId,
            CancellationToken cancellationToken = default)
    {
        /*
         * This distinguishes:
         *
         * missing reading             -> null / API 404
         * existing reading, no AI yet -> empty collection / API 200
         */
        var reading = await _readingRepository.GetByIdAsync(
            readingId,
            cancellationToken);

        if (reading is null)
        {
            return null;
        }

        return await _interpretationRepository.GetByReadingIdAsync(
            readingId,
            cancellationToken);
    }
}