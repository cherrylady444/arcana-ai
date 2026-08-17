using Tarot.Application.Cards;
using Tarot.Application.Spreads;
using Tarot.Domain;

namespace Tarot.Application.Readings;

/// <summary>
/// Creates readings from trusted built-in spreads or bounded custom spread
/// definitions. ASP.NET always owns card drawing and persistence.
/// </summary>
public sealed class CreateReadingService : IReadingService
{
    private const string DefaultSpreadKey = "three-card";

    private readonly ICardDrawingService _cardDrawingService;
    private readonly ISpreadCatalog _spreadCatalog;
    private readonly IReadingRepository _readingRepository;

    public CreateReadingService(
        ICardDrawingService cardDrawingService,
        ISpreadCatalog spreadCatalog,
        IReadingRepository readingRepository)
    {
        _cardDrawingService = cardDrawingService;
        _spreadCatalog = spreadCatalog;
        _readingRepository = readingRepository;
    }

    public async Task<CreateReadingResult> CreateReadingAsync(
        CreateReadingCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var spread = ResolveSpread(command);
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
                    Meaning: meaning);
            })
            .ToList();

        var userContext = string.IsNullOrWhiteSpace(command.UserContext)
            ? null
            : command.UserContext.Trim();

        var result = new CreateReadingResult(
            Id: Guid.NewGuid(),
            UserContext: userContext,
            SpreadKey: spread.Key,
            SpreadName: spread.Name,
            SpreadDescription: spread.Description,
            IsCustomSpread: !spread.IsBuiltIn,
            Cards: readingCards,
            CreatedAtUtc: DateTimeOffset.UtcNow);

        await _readingRepository.SaveAsync(result, cancellationToken);

        return result;
    }

    private SpreadDefinition ResolveSpread(CreateReadingCommand command)
    {
        if (command.CustomSpread is not null)
        {
            if (!string.IsNullOrWhiteSpace(command.SpreadKey))
            {
                throw new ArgumentException(
                    "Provide either spreadKey or customSpread, not both.",
                    nameof(command));
            }

            var customKey = $"custom:{Guid.NewGuid():N}";

            return CustomSpreadValidator.ValidateAndCreate(
                command.CustomSpread,
                customKey);
        }

        var spreadKey = string.IsNullOrWhiteSpace(command.SpreadKey)
            ? DefaultSpreadKey
            : command.SpreadKey.Trim();

        var spread = _spreadCatalog.GetByKey(spreadKey);

        if (spread is null)
        {
            throw new ArgumentException(
                $"Unknown spread key: {spreadKey}.",
                nameof(command.SpreadKey));
        }

        return spread;
    }
}
