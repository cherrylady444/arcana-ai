using Tarot.Application.Readings;

namespace Tarot.Tests.TestDoubles;

/// <summary>
/// Reusable in-memory test implementation of IReadingRepository.
///
/// Each test should create its own instance. Do not share one static
/// instance between tests.
/// 
/// This shared fake has three kinds of test support:
/// SavedReadings
///     → lets create tests inspect saved readings
/// ReadingToReturn
///     → lets get-by-ID tests configure a result
/// PageToReturn
///     → lets list tests configure a paginated result
/// 
/// Fake = a simplified but working implementation of a real interface;
///        does not use PostgreSQL. Instead, it stores readings in a normal in-memory list.
/// 
/// Spy = records information about how it was called so that the test can inspect it afterward.
/// </summary>
internal sealed class FakeReadingRepository : IReadingRepository
{
    public List<CreateReadingResult> SavedReadings { get; } = [];

    public ReadingDetails? ReadingToReturn { get; set; }

    public PagedReadingsResult PageToReturn { get; set; } =
        new(
            Items: [],
            Page: 1,
            PageSize: 20,
            TotalCount: 0,
            TotalPages: 0);

    public Guid? LastRequestedReadingId { get; private set; }

    public int? LastRequestedPage { get; private set; }

    public int? LastRequestedPageSize { get; private set; }

    public Task SaveAsync(
        CreateReadingResult reading,
        CancellationToken cancellationToken = default)
    {
        SavedReadings.Add(reading);

        return Task.CompletedTask;
    }

    public Task<ReadingDetails?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        LastRequestedReadingId = id;

        var result = ReadingToReturn?.Id == id
            ? ReadingToReturn
            : null;

        return Task.FromResult(result);
    }

    public Task<PagedReadingsResult> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        LastRequestedPage = page;
        LastRequestedPageSize = pageSize;

        return Task.FromResult(PageToReturn);
    }
}