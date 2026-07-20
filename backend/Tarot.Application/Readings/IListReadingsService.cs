namespace Tarot.Application.Readings;

public interface IListReadingsService
{
    Task<PagedReadingsResult> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}