using Tarot.Application.Readings;

namespace Tarot.Application.Interpretations;

/// <summary>
/// Loads a trusted persisted reading and sends its immutable snapshot to the
/// configured interpretation provider.
/// </summary>
/// <remarks>
/// This service is responsible for generating interpretations for persisted readings.
/// It ensures that the interpretation is generated from a trusted snapshot of the reading.
/// </remarks>
public sealed class GenerateReadingInterpretationService
    : IGenerateReadingInterpretationService
{
    private readonly IReadingRepository _readingRepository;
    private readonly IReadingInterpreter _readingInterpreter;

    public GenerateReadingInterpretationService(
        IReadingRepository readingRepository,
        IReadingInterpreter readingInterpreter)
    {
        _readingRepository = readingRepository;
        _readingInterpreter = readingInterpreter;
    }

    public async Task<ReadingInterpretation?> GenerateAsync(
        Guid readingId,
        CancellationToken cancellationToken = default)
    {
        var reading = await _readingRepository.GetByIdAsync(
            readingId,
            cancellationToken);

        if (reading is null)
        {
            return null;
        }

        /*
         * Construct the interpretation input entirely from the trusted
         * PostgreSQL snapshot.
         *
         * No card, orientation, meaning, or position data is accepted from
         * the browser.
         */
        var input = new ReadingInterpretationInput(
            ReadingId: reading.Id,
            UserContext: reading.UserContext,
            Spread: new InterpretationSpread(
                Key: reading.SpreadKey,
                Name: reading.SpreadName),
            Placements: reading.Cards
                .OrderBy(card => card.PositionOrder)
                .Select(card => new InterpretationPlacement(
                    PositionOrder: card.PositionOrder,
                    PositionKey: card.PositionKey,
                    PositionTitle: card.PositionTitle,
                    PositionPrompt: card.PositionPrompt,
                    PositionDescription: card.PositionDescription,
                    CardId: card.CardId,
                    CardName: card.CardName,
                    Arcana: card.Arcana,
                    Suit: card.Suit,
                    Orientation: card.Orientation,
                    Meaning: card.Meaning))
                .ToArray());

        var interpretation = await _readingInterpreter.InterpretAsync(
            input,
            cancellationToken);

        /*
         * Python already validates this rule, but ASP.NET validates it again
         * before exposing the result publicly.
         */
        ValidatePositionReferences(input, interpretation);

        return new ReadingInterpretation(
            OverallTheme: interpretation.OverallTheme,
            PositionInterpretations: interpretation.PositionInterpretations
                .OrderBy(item => item.PositionOrder)
                .ToArray(),
            Synthesis: interpretation.Synthesis,
            ReflectionQuestions: interpretation.ReflectionQuestions);
    }

    private static void ValidatePositionReferences(
        ReadingInterpretationInput input,
        ReadingInterpretation interpretation)
    {
        var expectedOrders = input.Placements
            .Select(placement => placement.PositionOrder)
            .ToHashSet();

        var returnedOrders = interpretation.PositionInterpretations
            .Select(item => item.PositionOrder)
            .ToArray();

        if (returnedOrders.Length != returnedOrders.Distinct().Count())
        {
            throw new ReadingInterpreterResponseException(
                "The interpretation contains duplicate position references.");
        }

        if (!expectedOrders.SetEquals(returnedOrders))
        {
            throw new ReadingInterpreterResponseException(
                "The interpretation positions do not match the reading positions.");
        }
    }
}