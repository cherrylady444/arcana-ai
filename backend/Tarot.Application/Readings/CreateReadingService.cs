using Tarot.Application.Cards;
using Tarot.Application.Spreads;
using Tarot.Domain;

namespace Tarot.Application.Readings;

/// <summary>
/// Service responsible for creating tarot readings.
/// </summary>
/// <remarks>
/// The service chooses the requested spread, draws the required number of cards,
/// assigns each drawn card to the matching spread position, applies the
/// orientation-specific meaning, and preserves optional user context.
/// </remarks>
public sealed class CreateReadingService : IReadingService
{
    private const string DefaultSpreadKey = "three-card";

    private readonly ICardDrawingService _cardDrawingService;
    private readonly ISpreadCatalog _spreadCatalog;

    public CreateReadingService(
        ICardDrawingService cardDrawingService,
        ISpreadCatalog spreadCatalog)
    {
        _cardDrawingService = cardDrawingService;
        _spreadCatalog = spreadCatalog;
    }

    public CreateReadingResult CreateReading(CreateReadingCommand command)
    {
        var spreadKey = string.IsNullOrWhiteSpace(command.SpreadKey)
            ? DefaultSpreadKey
            : command.SpreadKey.Trim();

        var spread = _spreadCatalog.GetByKey(spreadKey);

        if (spread is null)
        {
            throw new ArgumentException($"Unknown spread key: {spreadKey}.", nameof(command.SpreadKey));
        }

        var drawnCards = _cardDrawingService.DrawCards(spread.CardCount);

        var orderedPositions = spread.Positions
            .OrderBy(position => position.Order)
            .ToList();

        var readingCards = drawnCards
            .Select((drawnCard, index) =>
            {
                var meaning = drawnCard.Orientation == CardOrientation.Upright
                    ? drawnCard.Card.UprightMeaning
                    : drawnCard.Card.ReversedMeaning;

                return new ReadingCard(
                    Card: drawnCard.Card,
                    Orientation: drawnCard.Orientation,
                    Position: orderedPositions[index],
                    Meaning: meaning
                );
            })
            .ToList();

        var userContext = string.IsNullOrWhiteSpace(command.UserContext)
            ? null
            : command.UserContext.Trim();

        return new CreateReadingResult(
            Id: Guid.NewGuid(),
            UserContext: userContext,
            SpreadKey: spread.Key,
            SpreadName: spread.Name,
            Cards: readingCards,
            CreatedAtUtc: DateTimeOffset.UtcNow
        );
    }
}
