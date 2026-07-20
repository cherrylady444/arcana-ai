namespace Tarot.Application.Readings;

public interface IReadingRepository
{
    Task SaveAsync(
        CreateReadingResult reading,
        CancellationToken cancellationToken = default);

    Task<ReadingDetails?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PagedReadingsResult> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
        
}