using Tarot.Application.Cards;
using Tarot.Application.Readings;
using Tarot.Application.Spreads;
using Tarot.Domain;

using Tarot.Tests.TestDoubles;

namespace Tarot.Tests.Readings;

public sealed class CreateReadingServiceTests
{
    [Fact]
    public async Task CreateReadingAsync_WhenSpreadKeyIsMissing_UsesThreeCardSpread()
    {
        var service = CreateService();

        var result = await service.CreateReadingAsync(new CreateReadingCommand());

        Assert.Null(result.UserContext);
        Assert.Equal("three-card", result.SpreadKey);
        Assert.Equal("Three-Card Spread", result.SpreadName);
        Assert.Equal(3, result.Cards.Count);
    }

    [Fact]
    public async Task CreateReadingAsync_WhenSpreadKeyIsOneCard_ReturnsOneCard()
    {
        var service = CreateService();

        var result = await service.CreateReadingAsync(
            new CreateReadingCommand(SpreadKey: "one-card")
        );

        Assert.Equal("one-card", result.SpreadKey);
        Assert.Single(result.Cards);
    }

    [Fact]
    public async Task CreateReadingAsync_WhenSpreadKeyIsCelticCross_ReturnsTenCards()
    {
        var service = CreateService();

        var result = await service.CreateReadingAsync(
            new CreateReadingCommand(SpreadKey: "celtic-cross")
        );

        Assert.Equal("celtic-cross", result.SpreadKey);
        Assert.Equal(10, result.Cards.Count);
    }

    [Fact]
    public async Task CreateReadingAsync_AssignsSpreadPositionsInOrder()
    {
        var service = CreateService();

        var result = await service.CreateReadingAsync(
            new CreateReadingCommand(SpreadKey: "three-card")
        );

        var positions = result.Cards
            .OrderBy(card => card.Position.Order)
            .Select(card => card.Position.Key)
            .ToList();

        Assert.Equal(new[] { "past", "present", "near-future" }, positions);
    }

    [Fact]
    public async Task CreateReadingAsync_ReturnsUniqueCards()
    {
        var service = CreateService();

        var result = await service.CreateReadingAsync(
            new CreateReadingCommand(SpreadKey: "five-card")
        );

        var uniqueCardIds = result.Cards
            .Select(readingCard => readingCard.Card.Id)
            .Distinct()
            .Count();

        Assert.Equal(result.Cards.Count, uniqueCardIds);
    }

    [Fact]
    public async Task CreateReadingAsync_DoesNotRequireUserContext()
    {
        var service = CreateService();

        var result = await service.CreateReadingAsync(
            new CreateReadingCommand(SpreadKey: "one-card")
        );

        Assert.Null(result.UserContext);
        Assert.Equal("one-card", result.SpreadKey);
        Assert.Single(result.Cards);
    }

    [Fact]
    public async Task CreateReadingAsync_WhenUserContextIsProvided_TrimsAndPreservesIt()
    {
        var service = CreateService();

        var result = await service.CreateReadingAsync(
            new CreateReadingCommand(
                SpreadKey: "three-card-love",
                UserContext: "  General relationship insight  "
            )
        );

        Assert.Equal("General relationship insight", result.UserContext);
    }

    [Fact]
    public async Task CreateReadingAsync_WhenUserContextIsWhitespace_TreatsItAsNull()
    {
        var service = CreateService();

        var result = await service.CreateReadingAsync(
            new CreateReadingCommand(
                SpreadKey: "three-card",
                UserContext: "   "
            )
        );

        Assert.Null(result.UserContext);
    }

    [Fact]
    public async Task CreateReadingAsync_WhenSpreadKeyIsUnknown_ThrowsException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateReadingAsync(new CreateReadingCommand(SpreadKey: "unknown-spread"))
        );
    }

    [Fact]
    public async Task CreateReadingAsync_UsesReversedMeaningForReversedCards()
    {
        var card = new TarotCard
        {
            Id = Guid.NewGuid(),
            Name = "Test Card",
            Arcana = "Test Arcana",
            Suit = "Test Suit",
            UprightMeaning = "Upright test meaning",
            ReversedMeaning = "Reversed test meaning"
        };

        var service = CreateService(
            cardDrawingService: new FixedCardDrawingService(new DrawnCard(card, CardOrientation.Reversed))
        );

        var result = await service.CreateReadingAsync(new CreateReadingCommand(SpreadKey: "one-card"));

        Assert.Equal("Reversed test meaning", result.Cards.Single().Meaning);
    }

    [Fact]
    public async Task CreateReadingAsync_SavesCreatedReading()
    {
        var repository = new FakeReadingRepository();
        var service = CreateService(repository: repository);

        var result = await service.CreateReadingAsync(
            new CreateReadingCommand(SpreadKey: "three-card")
        );

        Assert.Single(repository.SavedReadings);
        Assert.Equal(result.Id, repository.SavedReadings[0].Id);
        Assert.Equal("three-card", repository.SavedReadings[0].SpreadKey);
        Assert.Equal(3, repository.SavedReadings[0].Cards.Count);
    }


    [Fact]
    public async Task CreateReadingAsync_WithCustomSpread_DrawsOneCardPerPosition()
    {
        var service = CreateService();

        var customSpread = new CustomSpreadDefinition(
            Name: "Decision Lens",
            Description: "A custom spread for a specific decision.",
            Positions:
            [
                new CustomSpreadPositionDefinition(1, "Need", "What need is driving this?", "The underlying need."),
                new CustomSpreadPositionDefinition(2, "Risk", "What risk am I overlooking?", "A possible blind spot."),
                new CustomSpreadPositionDefinition(3, "Direction", "What should guide me?", "A useful guiding perspective.")
            ]);

        var result = await service.CreateReadingAsync(
            new CreateReadingCommand(
                UserContext: "Should I make this change?",
                CustomSpread: customSpread));

        Assert.True(result.IsCustomSpread);
        Assert.StartsWith("custom:", result.SpreadKey);
        Assert.Equal("Decision Lens", result.SpreadName);
        Assert.Equal(customSpread.Description, result.SpreadDescription);
        Assert.Equal(3, result.Cards.Count);
        Assert.Equal(
            new[] { "position-1", "position-2", "position-3" },
            result.Cards
                .OrderBy(card => card.Position.Order)
                .Select(card => card.Position.Key));
    }

    [Fact]
    public async Task CreateReadingAsync_WhenCustomSpreadHasMoreThanTenPositions_Throws()
    {
        var service = CreateService();
        var positions = Enumerable.Range(1, 11)
            .Select(order => new CustomSpreadPositionDefinition(
                order,
                $"Position {order}",
                $"What does position {order} explore?",
                $"Description {order}."))
            .ToArray();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateReadingAsync(
                new CreateReadingCommand(
                    CustomSpread: new CustomSpreadDefinition(
                        "Too Large",
                        "This spread should be rejected.",
                        positions))));
    }

    [Fact]
    public async Task CreateReadingAsync_WhenBothExistingAndCustomSpreadAreProvided_Throws()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateReadingAsync(
                new CreateReadingCommand(
                    SpreadKey: "three-card",
                    CustomSpread: new CustomSpreadDefinition(
                        "Custom",
                        "Custom description.",
                        [new CustomSpreadPositionDefinition(1, "Focus", "What matters?", "The main focus.")]))));
    }

    private static CreateReadingService CreateService(
        ICardDrawingService? cardDrawingService = null,
        ISpreadCatalog? spreadCatalog = null,
        FakeReadingRepository? repository = null)
    {
        return new CreateReadingService(
            cardDrawingService ?? new CardDrawingService(),
            spreadCatalog ?? new BuiltInSpreadCatalog(),
            repository ?? new FakeReadingRepository()
        );
    }

    private sealed class FixedCardDrawingService : ICardDrawingService
    {
        private readonly IReadOnlyList<DrawnCard> _cards;

        public FixedCardDrawingService(params DrawnCard[] cards)
        {
            _cards = cards;
        }

        public IReadOnlyList<DrawnCard> DrawCards(int count)
        {
            if (count > _cards.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "The fixed test deck does not contain enough cards.");
            }

            return _cards.Take(count).ToList();
        }
    }
}
