using Tarot.Application.Cards;

namespace Tarot.Application.Readings;

/// <summary>
/// Service responsible for creating tarot readings.
/// </summary>
/// <remarks>
/// This service handles the creation of tarot readings based on user questions.
/// It utilizes the <see cref="ICardDrawingService"/> to draw cards for the reading.
/// </remarks>  

public sealed class CreateReadingService : IReadingService
{
    private const int ThreeCardReadingCount = 3;

    private readonly ICardDrawingService _cardDrawingService;

    public CreateReadingService(ICardDrawingService cardDrawingService)
    {
        _cardDrawingService = cardDrawingService;
    }

    public CreateReadingResult CreateReading(CreateReadingCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Question))
        {
            throw new ArgumentException("Question is required.", nameof(command.Question));
        }

        var cards = _cardDrawingService.DrawCards(ThreeCardReadingCount);

        return new CreateReadingResult(
            Id: Guid.NewGuid(),
            Question: command.Question.Trim(),
            Cards: cards,
            CreatedAtUtc: DateTimeOffset.UtcNow
        );
    }
}
