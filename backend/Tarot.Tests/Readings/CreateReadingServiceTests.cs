using Tarot.Application.Cards;
using Tarot.Application.Readings;
using Tarot.Application.Spreads;
using Tarot.Domain;

namespace Tarot.Tests.Readings;

public sealed class CreateReadingServiceTests
{
    [Fact]
    public void CreateReading_WhenSpreadKeyIsMissing_UsesThreeCardSpread()
    {
        var service = CreateService();

        var result = service.CreateReading(new CreateReadingCommand());

        Assert.Null(result.UserContext);
        Assert.Equal("three-card", result.SpreadKey);
        Assert.Equal("Three-Card Spread", result.SpreadName);
        Assert.Equal(3, result.Cards.Count);
    }

    [Fact]
    public void CreateReading_WhenSpreadKeyIsOneCard_ReturnsOneCard()
    {
        var service = CreateService();

        var result = service.CreateReading(
            new CreateReadingCommand(SpreadKey: "one-card")
        );

        Assert.Equal("one-card", result.SpreadKey);
        Assert.Single(result.Cards);
    }

    [Fact]
    public void CreateReading_WhenSpreadKeyIsCelticCross_ReturnsTenCards()
    {
        var service = CreateService();

        var result = service.CreateReading(
            new CreateReadingCommand(SpreadKey: "celtic-cross")
        );

        Assert.Equal("celtic-cross", result.SpreadKey);
        Assert.Equal(10, result.Cards.Count);
    }

    [Fact]
    public void CreateReading_AssignsSpreadPositionsInOrder()
    {
        var service = CreateService();

        var result = service.CreateReading(
            new CreateReadingCommand(SpreadKey: "three-card")
        );

        var positions = result.Cards
            .OrderBy(card => card.Position.Order)
            .Select(card => card.Position.Key)
            .ToList();

        Assert.Equal(new[] { "past", "present", "near-future" }, positions);
    }

    [Fact]
    public void CreateReading_ReturnsUniqueCards()
    {
        var service = CreateService();

        var result = service.CreateReading(
            new CreateReadingCommand(SpreadKey: "five-card")
        );

        var uniqueCardIds = result.Cards
            .Select(readingCard => readingCard.Card.Id)
            .Distinct()
            .Count();

        Assert.Equal(result.Cards.Count, uniqueCardIds);
    }

    [Fact]
    public void CreateReading_DoesNotRequireUserContext()
    {
        var service = CreateService();

        var result = service.CreateReading(
            new CreateReadingCommand(SpreadKey: "one-card")
        );

        Assert.Null(result.UserContext);
        Assert.Equal("one-card", result.SpreadKey);
        Assert.Single(result.Cards);
    }

    [Fact]
    public void CreateReading_WhenUserContextIsProvided_TrimsAndPreservesIt()
    {
        var service = CreateService();

        var result = service.CreateReading(
            new CreateReadingCommand(
                SpreadKey: "three-card-love",
                UserContext: "  General relationship insight  "
            )
        );

        Assert.Equal("General relationship insight", result.UserContext);
    }

    [Fact]
    public void CreateReading_WhenUserContextIsWhitespace_TreatsItAsNull()
    {
        var service = CreateService();

        var result = service.CreateReading(
            new CreateReadingCommand(
                SpreadKey: "three-card",
                UserContext: "   "
            )
        );

        Assert.Null(result.UserContext);
    }

    [Fact]
    public void CreateReading_WhenSpreadKeyIsUnknown_ThrowsException()
    {
        var service = CreateService();

        Assert.Throws<ArgumentException>(() =>
            service.CreateReading(new CreateReadingCommand(SpreadKey: "unknown-spread"))
        );
    }

    [Fact]
    public void CreateReading_UsesReversedMeaningForReversedCards()
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

        var service = new CreateReadingService(
            new FixedCardDrawingService(new DrawnCard(card, CardOrientation.Reversed)),
            new BuiltInSpreadCatalog()
        );

        var result = service.CreateReading(new CreateReadingCommand(SpreadKey: "one-card"));

        Assert.Equal("Reversed test meaning", result.Cards.Single().Meaning);
    }

    private static CreateReadingService CreateService()
    {
        return new CreateReadingService(
            new CardDrawingService(),
            new BuiltInSpreadCatalog()
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
            return _cards.Take(count).ToList();
        }
    }
}
