using Tarot.Application.Readings;
using Tarot.Tests.TestDoubles;

namespace Tarot.Tests.Readings;

public sealed class GetReadingByIdServiceTests
{
    [Fact]
    public async Task GetByIdAsync_WhenReadingExists_ReturnsReading()
    {
        var reading = CreateReadingDetails();

        var repository = new FakeReadingRepository
        {
            ReadingToReturn = reading
        };

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
        var repository = new FakeReadingRepository();
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
}