using System.Security.Cryptography;
using Tarot.Domain;

namespace Tarot.Application.Cards;

public sealed class CardDrawingService : ICardDrawingService
{
    public IReadOnlyList<DrawnCard> DrawCards(int count)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Card count must be greater than zero.");
        }

        if (count > TarotDeck.AllCards.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Card count cannot be greater than the number of cards in the deck.");
        }

        var availableCards = TarotDeck.AllCards.ToList();
        var drawnCards = new List<DrawnCard>(capacity: count);

        for (var i = 0; i < count; i++)
        {
            var selectedIndex = RandomNumberGenerator.GetInt32(availableCards.Count);
            var selectedCard = availableCards[selectedIndex];

            availableCards.RemoveAt(selectedIndex);

            drawnCards.Add(new DrawnCard(
                selectedCard,
                DrawOrientation()
            ));
        }

        return drawnCards;
    }

    private static CardOrientation DrawOrientation()
    {
        var value = RandomNumberGenerator.GetInt32(2);

        return value == 0
            ? CardOrientation.Upright
            : CardOrientation.Reversed;
    }
}
