using Microsoft.EntityFrameworkCore;
using Tarot.Infrastructure.Persistence;
using Tarot.Infrastructure.Persistence.Entities;
using Tarot.Infrastructure.Persistence.Repositories;

namespace Tarot.Tests.Readings;

public sealed class PostgresReadingRepositoryTests
{
	[Fact]
	public async Task GetPageAsync_WhenDatabaseIsEmpty_ReturnsEmptyPage()
	{
		await using var dbContext = CreateDbContext();
		var repository = new PostgresReadingRepository(dbContext);

		var result = await repository.GetPageAsync(page: 1, pageSize: 20);

		Assert.Empty(result.Items);
		Assert.Equal(1, result.Page);
		Assert.Equal(20, result.PageSize);
		Assert.Equal(0, result.TotalCount);
		Assert.Equal(0, result.TotalPages);
	}

	[Fact]
	public async Task GetPageAsync_ReturnsReadingsOrderedNewestFirst()
	{
		await using var dbContext = CreateDbContext();

		var oldest = CreateReadingEntity(new DateTimeOffset(2026, 7, 16, 10, 0, 0, TimeSpan.Zero));
		var newest = CreateReadingEntity(new DateTimeOffset(2026, 7, 18, 10, 0, 0, TimeSpan.Zero));
		var middle = CreateReadingEntity(new DateTimeOffset(2026, 7, 17, 10, 0, 0, TimeSpan.Zero));

		dbContext.Readings.AddRange(oldest, newest, middle);
		await dbContext.SaveChangesAsync();

		var repository = new PostgresReadingRepository(dbContext);

		var result = await repository.GetPageAsync(page: 1, pageSize: 20);

		Assert.Equal(
			new[] { newest.Id, middle.Id, oldest.Id },
			result.Items.Select(reading => reading.Id));
	}

	[Fact]
	public async Task GetPageAsync_AdjacentPagesDoNotContainDuplicateReadings()
	{
		await using var dbContext = CreateDbContext();

		var readings = Enumerable.Range(0, 5)
			.Select(index => CreateReadingEntity(
				new DateTimeOffset(2026, 7, 18, 12, 0, 0, TimeSpan.Zero)
					.AddMinutes(-index)))
			.ToList();

		dbContext.Readings.AddRange(readings);
		await dbContext.SaveChangesAsync();

		var repository = new PostgresReadingRepository(dbContext);

		var firstPage = await repository.GetPageAsync(page: 1, pageSize: 2);
		var secondPage = await repository.GetPageAsync(page: 2, pageSize: 2);

		var firstPageIds = firstPage.Items.Select(reading => reading.Id).ToHashSet();
		var secondPageIds = secondPage.Items.Select(reading => reading.Id).ToHashSet();

		Assert.Equal(2, firstPage.Items.Count);
		Assert.Equal(2, secondPage.Items.Count);
		Assert.Empty(firstPageIds.Intersect(secondPageIds));
		Assert.Equal(5, firstPage.TotalCount);
		Assert.Equal(3, firstPage.TotalPages);
		Assert.Equal(5, secondPage.TotalCount);
		Assert.Equal(3, secondPage.TotalPages);
	}

	private static TarotDbContext CreateDbContext()
	{
		var options = new DbContextOptionsBuilder<TarotDbContext>()
			.UseInMemoryDatabase($"tarot-tests-{Guid.NewGuid()}")
			.Options;

		return new TarotDbContext(options);
	}

	private static ReadingEntity CreateReadingEntity(
		DateTimeOffset createdAtUtc,
		int cardCount = 1)
	{
		var readingId = Guid.NewGuid();

		return new ReadingEntity
		{
			Id = readingId,
			UserContext = "Test reading",
			SpreadKey = cardCount == 1 ? "one-card" : "custom-spread",
			SpreadName = cardCount == 1 ? "One-Card Reading" : "Custom Spread",
			CreatedAtUtc = createdAtUtc,
			Cards = Enumerable.Range(1, cardCount)
				.Select(positionOrder => new ReadingCardEntity
				{
					Id = Guid.NewGuid(),
					ReadingId = readingId,
					PositionOrder = positionOrder,
					PositionKey = $"position-{positionOrder}",
					PositionTitle = $"Position {positionOrder}",
					PositionPrompt = "Test prompt",
					PositionDescription = "Test description",
					CardId = Guid.NewGuid(),
					CardName = $"Test Card {positionOrder}",
					Arcana = "Major Arcana",
					Suit = null,
					Orientation = "Upright",
					Meaning = "Test meaning"
				})
				.ToList()
		};
	}
}