using Microsoft.EntityFrameworkCore;
using Tarot.Application.Readings;
using Tarot.Domain;
using Tarot.Infrastructure.Persistence.Entities;
using Tarot.Infrastructure.Persistence.Repositories;
using Tarot.IntegrationTests.Infrastructure;
using Xunit;

namespace Tarot.IntegrationTests.Readings;

/// <summary>
/// Tests PostgresReadingRepository against a real temporary PostgreSQL
/// database.
///
/// These are integration tests rather than unit tests because they exercise:
///
/// - PostgresReadingRepository;
/// - TarotDbContext;
/// - EF Core;
/// - the Npgsql PostgreSQL provider;
/// - the real EF Core migrations;
/// - an actual PostgreSQL server.
///
/// The PostgreSqlFixture starts one PostgreSQL container for this test class.
/// </summary>
public sealed class PostgresReadingRepositoryTests :
    IClassFixture<PostgreSqlFixture>,
    IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;

    public PostgresReadingRepositoryTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>
    /// xUnit calls this method before every test in this class.
    ///
    /// Recreating the database before each test guarantees test isolation:
    /// every test begins with the real schema but no existing reading data.
    /// </summary>
    public Task InitializeAsync()
    {
        return _fixture.RecreateDatabaseAsync();
    }

    /// <summary>
    /// No per-test cleanup is required because the next test recreates the
    /// database and the fixture removes the entire container at the end.
    /// </summary>
    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetPageAsync_WhenDatabaseIsEmpty_ReturnsEmptyPage()
    {
        await using var dbContext = _fixture.CreateDbContext();

        var repository = new PostgresReadingRepository(dbContext);

        // The database has migrations applied, but it contains no readings.
        var result = await repository.GetPageAsync(
            page: 1,
            pageSize: 20);

        Assert.Empty(result.Items);
        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public async Task GetPageAsync_ReturnsReadingsOrderedNewestFirst()
    {
        await using var dbContext = _fixture.CreateDbContext();

        /*
         * Insert the entities in a deliberately mixed order.
         *
         * The repository should not depend on insertion order. It must ask
         * PostgreSQL to sort them by CreatedAtUtc descending.
         */
        var oldest = CreateReadingEntity(
            new DateTimeOffset(
                2026, 7, 16, 10, 0, 0, TimeSpan.Zero));

        var newest = CreateReadingEntity(
            new DateTimeOffset(
                2026, 7, 18, 10, 0, 0, TimeSpan.Zero));

        var middle = CreateReadingEntity(
            new DateTimeOffset(
                2026, 7, 17, 10, 0, 0, TimeSpan.Zero));

        dbContext.Readings.AddRange(oldest, newest, middle);
        await dbContext.SaveChangesAsync();

        var repository = new PostgresReadingRepository(dbContext);

        var result = await repository.GetPageAsync(
            page: 1,
            pageSize: 20);

        Assert.Equal(
            new[] { newest.Id, middle.Id, oldest.Id },
            result.Items.Select(reading => reading.Id));
    }

    [Fact]
    public async Task GetPageAsync_AdjacentPagesDoNotContainDuplicateReadings()
    {
        await using var dbContext = _fixture.CreateDbContext();

        /*
         * Create five readings with distinct timestamps.
         *
         * With a page size of two:
         *
         * page 1 should contain items 1-2;
         * page 2 should contain items 3-4;
         * page 3 should contain item 5.
         */
        var readings = Enumerable.Range(0, 5)
            .Select(index => CreateReadingEntity(
                new DateTimeOffset(
                        2026, 7, 18, 12, 0, 0, TimeSpan.Zero)
                    .AddMinutes(-index)))
            .ToList();

        dbContext.Readings.AddRange(readings);
        await dbContext.SaveChangesAsync();

        var repository = new PostgresReadingRepository(dbContext);

        var firstPage = await repository.GetPageAsync(
            page: 1,
            pageSize: 2);

        var secondPage = await repository.GetPageAsync(
            page: 2,
            pageSize: 2);

        var firstPageIds = firstPage.Items
            .Select(reading => reading.Id)
            .ToHashSet();

        var secondPageIds = secondPage.Items
            .Select(reading => reading.Id)
            .ToHashSet();

        Assert.Equal(2, firstPage.Items.Count);
        Assert.Equal(2, secondPage.Items.Count);

        // Intersect returns IDs appearing in both pages. It must be empty.
        Assert.Empty(firstPageIds.Intersect(secondPageIds));

        Assert.Equal(5, firstPage.TotalCount);
        Assert.Equal(3, firstPage.TotalPages);

        Assert.Equal(5, secondPage.TotalCount);
        Assert.Equal(3, secondPage.TotalPages);
    }

    [Fact]
    public async Task SaveAsync_ThenGetByIdAsync_ReturnsSavedReading()
    {
        var reading = CreateApplicationReading(
            createdAtUtc: new DateTimeOffset(
                2026, 7, 18, 14, 30, 0, TimeSpan.Zero),
            positionOrders: [1, 2, 3]);

        /*
         * Save and read through different DbContext instances.
         *
         * This is important because it proves that GetByIdAsync does not obtain
         * the original tracked C# object from the saving DbContext. The second
         * repository must load the data again from PostgreSQL.
         */
        await using (var saveDbContext = _fixture.CreateDbContext())
        {
            var saveRepository =
                new PostgresReadingRepository(saveDbContext);

            await saveRepository.SaveAsync(reading);
        }

        await using var readDbContext = _fixture.CreateDbContext();

        var readRepository =
            new PostgresReadingRepository(readDbContext);

        var result = await readRepository.GetByIdAsync(reading.Id);

        Assert.NotNull(result);

        Assert.Equal(reading.Id, result!.Id);
        Assert.Equal(reading.UserContext, result.UserContext);
        Assert.Equal(reading.SpreadKey, result.SpreadKey);
        Assert.Equal(reading.SpreadName, result.SpreadName);
        Assert.Equal(reading.SpreadDescription, result.SpreadDescription);
        Assert.Equal(reading.IsCustomSpread, result.IsCustomSpread);
        Assert.Equal(reading.CreatedAtUtc, result.CreatedAtUtc);
        Assert.Equal(reading.Cards.Count, result.Cards.Count);

        var expectedCards = reading.Cards
            .OrderBy(card => card.Position.Order)
            .ToArray();

        var actualCards = result.Cards.ToArray();

        /*
         * Compare every persisted snapshot field.
         *
         * The repository saves the selected card meaning and orientation as
         * they existed when the reading was created.
         */
        for (var index = 0; index < expectedCards.Length; index++)
        {
            var expected = expectedCards[index];
            var actual = actualCards[index];

            Assert.Equal(
                expected.Position.Order,
                actual.PositionOrder);

            Assert.Equal(
                expected.Position.Key,
                actual.PositionKey);

            Assert.Equal(
                expected.Position.Title,
                actual.PositionTitle);

            Assert.Equal(
                expected.Position.Prompt,
                actual.PositionPrompt);

            Assert.Equal(
                expected.Position.Description,
                actual.PositionDescription);

            Assert.Equal(
                expected.Card.Id,
                actual.CardId);

            Assert.Equal(
                expected.Card.Name,
                actual.CardName);

            Assert.Equal(
                expected.Card.Arcana,
                actual.Arcana);

            Assert.Equal(
                expected.Card.Suit,
                actual.Suit);

            Assert.Equal(
                expected.Orientation.ToString(),
                actual.Orientation);

            Assert.Equal(
                expected.Meaning,
                actual.Meaning);
        }
    }

    [Fact]
    public async Task GetByIdAsync_WhenReadingDoesNotExist_ReturnsNull()
    {
        await using var dbContext = _fixture.CreateDbContext();

        var repository = new PostgresReadingRepository(dbContext);

        var nonexistentReadingId = Guid.NewGuid();

        var result = await repository.GetByIdAsync(
            nonexistentReadingId);

        Assert.Null(result);
    }

    [Fact]
    public async Task SaveAsync_PersistsCardsInPositionOrder()
    {
        /*
         * The cards are intentionally supplied in the wrong order:
         *
         * 3, 1, 2
         *
         * SaveAsync should preserve the position-order values, and
         * GetByIdAsync should return them ordered as 1, 2, 3.
         */
        var reading = CreateApplicationReading(
            createdAtUtc: new DateTimeOffset(
                2026, 7, 18, 15, 0, 0, TimeSpan.Zero),
            positionOrders: [3, 1, 2]);

        await using (var saveDbContext = _fixture.CreateDbContext())
        {
            var saveRepository =
                new PostgresReadingRepository(saveDbContext);

            await saveRepository.SaveAsync(reading);
        }

        await using var verificationDbContext =
            _fixture.CreateDbContext();
        
        /*
         * A relational database does not guarantee a natural row order.
         *
         * Therefore, this query orders by PositionOrder explicitly and verifies
         * that PostgreSQL contains the correct position values.
         */

        var storedCards = await verificationDbContext.ReadingCards
            .AsNoTracking()
            .Where(card => card.ReadingId == reading.Id)
            .OrderBy(card => card.PositionOrder)
            .Select(card => new
            {
                card.PositionOrder,
                card.PositionKey
            })
            .ToListAsync();

        Assert.Equal(
            new[] { 1, 2, 3 },
            storedCards.Select(card => card.PositionOrder));

        Assert.Equal(
            new[] { "position-1", "position-2", "position-3" },
            storedCards.Select(card => card.PositionKey));
        
        /*
         * Also verify the public repository read result.
         *
         * GetByIdAsync explicitly orders the saved cards by PositionOrder.
         */

        var verificationRepository =
            new PostgresReadingRepository(verificationDbContext);

        var retrieved =
            await verificationRepository.GetByIdAsync(reading.Id);

        Assert.NotNull(retrieved);

        Assert.Equal(
            new[] { 1, 2, 3 },
            retrieved!.Cards.Select(card => card.PositionOrder));
    }


    [Fact]
    public async Task DatabaseMigrations_CanBeAppliedToEmptyDatabase()
    {
        /*
         * InitializeAsync normally recreates and migrates the database before
         * each test.
         *
         * For this specific test, delete it again so MigrateAsync starts from a
         * database with no application schema.
         */
        await _fixture.DeleteDatabaseAsync();

        await using var dbContext = _fixture.CreateDbContext();

        // Apply the real migrations from Tarot.Infrastructure.
        await dbContext.Database.MigrateAsync();

        // The database should now be reachable.
        Assert.True(await dbContext.Database.CanConnectAsync());

        var appliedMigrations =
            await dbContext.Database.GetAppliedMigrationsAsync();

        var pendingMigrations =
            await dbContext.Database.GetPendingMigrationsAsync();

        // At least one migration should have been applied.
        Assert.NotEmpty(appliedMigrations);

        // Every known migration should now be applied.
        Assert.Empty(pendingMigrations);

        /*
         * These queries also prove that the migrations created both tables.
         * They would fail with a PostgreSQL "relation does not exist" error if
         * either table had not been created.
         */
        Assert.Equal(0, await dbContext.Readings.CountAsync());
        Assert.Equal(0, await dbContext.ReadingCards.CountAsync());
    }

    /// <summary>
    /// Creates a database entity for pagination tests.
    ///
    /// Pagination tests insert entities directly because their purpose is to
    /// test GetPageAsync, not SaveAsync.
    /// </summary>
    private static ReadingEntity CreateReadingEntity(
        DateTimeOffset createdAtUtc,
        int cardCount = 1)
    {
        var readingId = Guid.NewGuid();

        return new ReadingEntity
        {
            Id = readingId,
            UserContext = "Test reading",
            SpreadKey = cardCount == 1
                ? "one-card"
                : "custom-spread",
            SpreadName = cardCount == 1
                ? "One-Card Reading"
                : "Custom Spread",
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

    /// <summary>
    /// Creates the application-layer model accepted by
    /// PostgresReadingRepository.SaveAsync.
    ///
    /// The supplied positionOrders array controls the order in which cards are
    /// given to the repository. This allows the tests to supply both correctly
    /// ordered and deliberately unordered card collections.
    /// </summary>
    private static CreateReadingResult CreateApplicationReading(
        DateTimeOffset createdAtUtc,
        params int[] positionOrders)
    {
        var readingId = Guid.NewGuid();

        var cards = positionOrders
            .Select(positionOrder =>
            {
                var orientation = positionOrder % 2 == 0
                    ? CardOrientation.Reversed
                    : CardOrientation.Upright;

                var tarotCard = new TarotCard
                {
                    Id = Guid.NewGuid(),
                    Name = $"Test Card {positionOrder}",
                    Arcana = "Major Arcana",
                    Suit = null,
                    UprightMeaning =
                        $"Upright meaning {positionOrder}",
                    ReversedMeaning =
                        $"Reversed meaning {positionOrder}"
                };

                var position = new SpreadPositionDefinition(
                    Order: positionOrder,
                    Key: $"position-{positionOrder}",
                    Title: $"Position {positionOrder}",
                    Prompt: $"Prompt {positionOrder}",
                    Description: $"Description {positionOrder}");

                var selectedMeaning =
                    orientation == CardOrientation.Upright
                        ? tarotCard.UprightMeaning
                        : tarotCard.ReversedMeaning;

                return new ReadingCard(
                    Card: tarotCard,
                    Orientation: orientation,
                    Position: position,
                    Meaning: selectedMeaning);
            })
            .ToList();

        return new CreateReadingResult(
            Id: readingId,
            UserContext: "Integration-test reading",
            SpreadKey: positionOrders.Length == 1
                ? "one-card"
                : "custom-spread",
            SpreadName: positionOrders.Length == 1
                ? "One-Card Reading"
                : "Custom Spread",
            Cards: cards,
            CreatedAtUtc: createdAtUtc,
            SpreadDescription: positionOrders.Length == 1
                ? "A one-card test spread."
                : "An exact custom spread snapshot used by the integration test.",
            IsCustomSpread: positionOrders.Length != 1);
    }
}