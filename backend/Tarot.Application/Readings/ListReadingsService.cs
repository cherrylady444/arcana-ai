namespace Tarot.Application.Readings;

/// <summary>
/// This service is responsible for retrieving a paginated list of 
/// readings from the repository.
/// </summary>
public sealed class ListReadingsService : IListReadingsService
{
    private const int MaximumPageSize = 100;

    private readonly IReadingRepository _readingRepository;

    public ListReadingsService(IReadingRepository readingRepository)
    {
        _readingRepository = readingRepository;
    }

    public Task<PagedReadingsResult> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(page),
                "Page must be at least 1.");
        }

        if (pageSize < 1 || pageSize > MaximumPageSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                $"Page size must be between 1 and {MaximumPageSize}.");
        }

        return _readingRepository.GetPageAsync(
            page,
            pageSize,
            cancellationToken);
    }
}