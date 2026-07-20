using Tarot.Application.Readings;
using Tarot.Tests.TestDoubles;

namespace Tarot.Tests.Readings;

public sealed class ListReadingsServiceTests
{
    [Fact]
    public async Task GetPageAsync_WithDefaultPagination_ForwardsPageOneAndPageSizeTwenty()
    {
        var repository = new FakeReadingRepository
        {
            PageToReturn = CreatePagedResult(page: 1, pageSize: 20)
        };

        var service = new ListReadingsService(repository);

        await service.GetPageAsync(page: 1, pageSize: 20);

        Assert.Equal(1, repository.LastRequestedPage);
        Assert.Equal(20, repository.LastRequestedPageSize);
    }

    [Fact]
    public async Task GetPageAsync_WhenRepositoryReturnsResult_ReturnsThatResult()
    {
        var expected = CreatePagedResult(page: 1, pageSize: 5);

        var repository = new FakeReadingRepository
        {
            PageToReturn = expected
        };

        var service = new ListReadingsService(repository);

        var result = await service.GetPageAsync(page: 1, pageSize: 5);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task GetPageAsync_WhenPageIsZero_ThrowsArgumentOutOfRangeException()
    {
        var repository = new FakeReadingRepository();
        var service = new ListReadingsService(repository);

        var exception =
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                service.GetPageAsync(page: 0, pageSize: 20));

        Assert.Equal("page", exception.ParamName);
    }

    [Fact]
    public async Task GetPageAsync_WhenPageSizeIsZero_ThrowsArgumentOutOfRangeException()
    {
        var repository = new FakeReadingRepository();
        var service = new ListReadingsService(repository);

        var exception =
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                service.GetPageAsync(page: 1, pageSize: 0));

        Assert.Equal("pageSize", exception.ParamName);
    }

    [Fact]
    public async Task GetPageAsync_WhenPageSizeExceedsMaximum_ThrowsArgumentOutOfRangeException()
    {
        var repository = new FakeReadingRepository();
        var service = new ListReadingsService(repository);

        var exception =
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                service.GetPageAsync(page: 1, pageSize: 101));

        Assert.Equal("pageSize", exception.ParamName);
    }

    private static PagedReadingsResult CreatePagedResult(
        int page,
        int pageSize)
    {
        return new PagedReadingsResult(
            Items:
            [
                new ReadingSummary(
                    Id: Guid.NewGuid(),
                    UserContext: "Career direction",
                    SpreadKey: "one-card",
                    SpreadName: "One-Card Reading",
                    CardCount: 1,
                    CreatedAtUtc: DateTimeOffset.UtcNow)
            ],
            Page: page,
            PageSize: pageSize,
            TotalCount: 1,
            TotalPages: 1);
    }
}