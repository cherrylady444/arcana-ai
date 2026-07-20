namespace Tarot.Application.Readings;

/// <summary>
/// This record represents a paginated result of readings. It contains a collection of reading summaries, 
/// along with pagination information such as the current page, page size, total count of readings, and total number of pages.
/// </summary>
public sealed record PagedReadingsResult(
    IReadOnlyCollection<ReadingSummary> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);