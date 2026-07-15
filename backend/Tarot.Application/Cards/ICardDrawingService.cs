using Tarot.Domain;

namespace Tarot.Application.Cards;

public interface ICardDrawingService
{
    IReadOnlyList<DrawnCard> DrawCards(int count);
}
