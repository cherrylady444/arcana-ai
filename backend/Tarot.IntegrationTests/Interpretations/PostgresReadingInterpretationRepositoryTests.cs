using Tarot.Application.Interpretations;
using Tarot.Infrastructure.Persistence.Entities;
using Tarot.Infrastructure.Persistence.Repositories;
using Tarot.IntegrationTests.Infrastructure;
using Xunit;

namespace Tarot.IntegrationTests.Interpretations;

/// <summary>
/// Tests PostgresReadingInterpretationRepository against a real temporary
/// PostgreSQL database.
///
/// These tests exercise:
///
/// - PostgresReadingInterpretationRepository;
/// - TarotDbContext;
/// - EF Core;
/// - Npgsql;
/// - the real EF Core migrations;
/// - an actual PostgreSQL server supplied by Testcontainers.
/// </summary>
public sealed class PostgresReadingInterpretationRepositoryTests :
    IClassFixture<PostgreSqlFixture>,
    IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture;

    public PostgresReadingInterpretationRepositoryTests(
        PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync()
    {
        return _fixture.RecreateDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task SaveAsync_ThenGetByIdAsync_ReturnsSavedInterpretation()
    {
        var readingId = Guid.NewGuid();

        var interpretation = new ReadingInterpretationDetails(
            Id: Guid.NewGuid(),
            ReadingId: readingId,
            OverallTheme: "A period of deliberate transition.",
            PositionInterpretations:
            [
                new PositionInterpretation(
                    PositionOrder: 3,
                    Interpretation: "Interpretation for position three."),
                new PositionInterpretation(
                    PositionOrder: 1,
                    Interpretation: "Interpretation for position one."),
                new PositionInterpretation(
                    PositionOrder: 2,
                    Interpretation: "Interpretation for position two.")
            ],
            Synthesis:
                "The reading suggests reflecting carefully before changing direction.",
            ReflectionQuestions:
            [
                "What part of your current direction still feels meaningful?",
                "What would a deliberate change look like?",
                "What information do you still need before deciding?"
            ],
            CreatedAtUtc: new DateTimeOffset(
                2026,
                8,
                13,
                12,
                0,
                0,
                TimeSpan.Zero));

        /*
         * Save the parent reading first because reading_interpretations.reading_id
         * is a foreign key to readings.id.
         */
        await using (var saveDbContext = _fixture.CreateDbContext())
        {
            saveDbContext.Readings.Add(
                CreateReadingEntity(readingId));

            await saveDbContext.SaveChangesAsync();

            var repository =
                new PostgresReadingInterpretationRepository(
                    saveDbContext);

            await repository.SaveAsync(interpretation);
        }

        /*
         * Read through a fresh DbContext so the result must actually come from
         * PostgreSQL rather than EF Core's tracked entity state.
         */
        await using var readDbContext =
            _fixture.CreateDbContext();

        var readRepository =
            new PostgresReadingInterpretationRepository(
                readDbContext);

        var result = await readRepository.GetByIdAsync(
            readingId,
            interpretation.Id);

        Assert.NotNull(result);

        Assert.Equal(
            interpretation.Id,
            result!.Id);

        Assert.Equal(
            readingId,
            result.ReadingId);

        Assert.Equal(
            interpretation.OverallTheme,
            result.OverallTheme);

        Assert.Equal(
            interpretation.Synthesis,
            result.Synthesis);

        Assert.Equal(
            interpretation.CreatedAtUtc,
            result.CreatedAtUtc);

        /*
         * The supplied positions were deliberately 3, 1, 2.
         *
         * The repository contract should return them in position order.
         */
        Assert.Equal(
            new[] { 1, 2, 3 },
            result.PositionInterpretations
                .Select(item => item.PositionOrder));

        Assert.Equal(
            new[]
            {
                "Interpretation for position one.",
                "Interpretation for position two.",
                "Interpretation for position three."
            },
            result.PositionInterpretations
                .Select(item => item.Interpretation));

        /*
         * Reflection-question order is part of the persisted interpretation
         * resource and must survive the PostgreSQL round trip.
         */
        Assert.Equal(
            interpretation.ReflectionQuestions,
            result.ReflectionQuestions);
    }

    [Fact]
    public async Task GetByReadingIdAsync_ReturnsOnlyRequestedReadingNewestFirst()
    {
        var firstReadingId = Guid.NewGuid();
        var secondReadingId = Guid.NewGuid();

        var oldest = CreateInterpretation(
            readingId: firstReadingId,
            createdAtUtc: new DateTimeOffset(
                2026,
                8,
                13,
                10,
                0,
                0,
                TimeSpan.Zero),
            overallTheme: "Oldest interpretation");

        var newest = CreateInterpretation(
            readingId: firstReadingId,
            createdAtUtc: new DateTimeOffset(
                2026,
                8,
                13,
                12,
                0,
                0,
                TimeSpan.Zero),
            overallTheme: "Newest interpretation");

        var otherReadingInterpretation = CreateInterpretation(
            readingId: secondReadingId,
            createdAtUtc: new DateTimeOffset(
                2026,
                8,
                13,
                13,
                0,
                0,
                TimeSpan.Zero),
            overallTheme: "Different reading");

        await using (var saveDbContext = _fixture.CreateDbContext())
        {
            saveDbContext.Readings.AddRange(
                CreateReadingEntity(firstReadingId),
                CreateReadingEntity(secondReadingId));

            await saveDbContext.SaveChangesAsync();

            var repository =
                new PostgresReadingInterpretationRepository(
                    saveDbContext);

            /*
             * Save deliberately out of chronological order.
             *
             * Retrieval must depend on CreatedAtUtc, not insertion order.
             */
            await repository.SaveAsync(oldest);
            await repository.SaveAsync(otherReadingInterpretation);
            await repository.SaveAsync(newest);
        }

        await using var readDbContext =
            _fixture.CreateDbContext();

        var readRepository =
            new PostgresReadingInterpretationRepository(
                readDbContext);

        var result = await readRepository.GetByReadingIdAsync(
            firstReadingId);

        Assert.Equal(2, result.Count);

        Assert.Equal(
            new[]
            {
                newest.Id,
                oldest.Id
            },
            result.Select(interpretation => interpretation.Id));

        Assert.All(
            result,
            interpretation =>
                Assert.Equal(
                    firstReadingId,
                    interpretation.ReadingId));

        Assert.DoesNotContain(
            result,
            interpretation =>
                interpretation.Id ==
                otherReadingInterpretation.Id);
    }

    private static ReadingEntity CreateReadingEntity(
        Guid readingId)
    {
        return new ReadingEntity
        {
            Id = readingId,
            UserContext =
                "Integration-test context for interpretation persistence.",
            SpreadKey = "one-card",
            SpreadName = "One-Card Reading",
            CreatedAtUtc = new DateTimeOffset(
                2026,
                8,
                13,
                9,
                0,
                0,
                TimeSpan.Zero)
        };
    }

    private static ReadingInterpretationDetails CreateInterpretation(
        Guid readingId,
        DateTimeOffset createdAtUtc,
        string overallTheme)
    {
        return new ReadingInterpretationDetails(
            Id: Guid.NewGuid(),
            ReadingId: readingId,
            OverallTheme: overallTheme,
            PositionInterpretations:
            [
                new PositionInterpretation(
                    PositionOrder: 1,
                    Interpretation:
                        $"Position interpretation for {overallTheme}.")
            ],
            Synthesis:
                $"Synthesis for {overallTheme}.",
            ReflectionQuestions:
            [
                $"What can you reflect on regarding {overallTheme}?",
                $"What could you do next regarding {overallTheme}?"
            ],
            CreatedAtUtc: createdAtUtc);
    }
}