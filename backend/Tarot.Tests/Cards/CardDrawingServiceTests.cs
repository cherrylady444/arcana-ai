using Tarot.Application.Cards;
using Tarot.Domain;

namespace Tarot.Tests.Cards;

public sealed class CardDrawingServiceTests
{
    private readonly CardDrawingService _service = new();

    [Fact]
    public void DrawCards_ReturnsRequestedNumberOfCards()
    {
        var cards = _service.DrawCards(3);

        Assert.Equal(3, cards.Count);
    }

    [Fact]
    public void DrawCards_ReturnsUniqueCards()
    {
        var cards = _service.DrawCards(3);

        var uniqueCardIds = cards
            .Select(drawnCard => drawnCard.Card.Id)
            .Distinct()
            .Count();

        Assert.Equal(cards.Count, uniqueCardIds);
    }

    [Fact]
    public void DrawCards_CanDrawTheWholeDeckWithoutDuplicates()
    {
        var cards = _service.DrawCards(TarotDeck.AllCards.Count);

        var uniqueCardIds = cards
            .Select(drawnCard => drawnCard.Card.Id)
            .Distinct()
            .Count();

        Assert.Equal(TarotDeck.AllCards.Count, cards.Count);
        Assert.Equal(TarotDeck.AllCards.Count, uniqueCardIds);
    }

    [Fact]
    public void DrawCards_AssignsOnlyValidOrientations()
    {
        var cards = _service.DrawCards(10);

        Assert.All(cards, drawnCard =>
        {
            Assert.True(
                drawnCard.Orientation is CardOrientation.Upright or CardOrientation.Reversed
            );
        });
    }

    [Fact]
    public void DrawCards_WhenCountIsZero_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.DrawCards(0));
    }

    [Fact]
    public void DrawCards_WhenCountIsGreaterThanDeckSize_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _service.DrawCards(TarotDeck.AllCards.Count + 1)
        );
    }
}
