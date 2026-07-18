using Tarot.Application.Readings;

namespace Tarot.Tests.Readings;

public sealed class GetReadingByIdServiceTests
{
    [Fact]
    public async Task GetByIdAsync_WhenReadingExists_ReturnsReading()
    {
        var reading = CreateReadingDetails();
        var repository = new FakeReadingRepository(reading);
        var service = new GetReadingByIdService(repository);

        var result = await service.GetByIdAsync(reading.Id);

        Assert.NotNull(result);
        Assert.Equal(reading.Id, result!.Id);
        Assert.Equal("one-card", result.SpreadKey);
        Assert.Single(result.Cards);
    }

    [Fact]
    public async Task GetByIdAsync_WhenReadingDoesNotExist_ReturnsNull()
    {
        var repository = new FakeReadingRepository(reading: null);
        var service = new GetReadingByIdService(repository);

        var result = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    private static ReadingDetails CreateReadingDetails()
    {
        return new ReadingDetails(
            Id: Guid.NewGuid(),
            UserContext: "Career direction",
            SpreadKey: "one-card",
            SpreadName: "One-Card Reading",
            Cards:
            [
                new ReadingCardDetails(
                    PositionOrder: 1,
                    PositionKey: "focus",
                    PositionTitle: "Focus",
                    PositionPrompt: "What deserves attention now?",
                    PositionDescription: "The central energy of the reading.",
                    CardId: Guid.NewGuid(),
                    CardName: "The Hermit",
                    Arcana: "Major Arcana",
                    Suit: null,
                    Orientation: "Upright",
                    Meaning: "Reflection and inner guidance"
                )
            ],
            CreatedAtUtc: DateTimeOffset.UtcNow
        );
    }

    private sealed class FakeReadingRepository : IReadingRepository
    {
        private readonly ReadingDetails? _reading;

        public FakeReadingRepository(ReadingDetails? reading)
        {
            _reading = reading;
        }

        public Task SaveAsync(
            CreateReadingResult reading,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<ReadingDetails?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = _reading?.Id == id
                ? _reading
                : null;

            return Task.FromResult(result);
        }
    }
}