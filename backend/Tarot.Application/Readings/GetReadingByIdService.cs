namespace Tarot.Application.Readings;

/// <summary>
/// Retrieves a previously saved tarot reading by its unique identifier.
/// </summary>
public sealed class GetReadingByIdService : IGetReadingByIdService
{
    private readonly IReadingRepository _readingRepository;

    public GetReadingByIdService(IReadingRepository readingRepository)
    {
        _readingRepository = readingRepository;
    }

    public Task<ReadingDetails?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _readingRepository.GetByIdAsync(id, cancellationToken);
    }
}