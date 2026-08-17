using Tarot.Application.Readings;

namespace Tarot.Application.Interpretations;

/// <summary>
/// Loads a trusted persisted reading,  requests an AI interpretation,
/// validates the result, and persists it as a new immutable resource.
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
    private readonly IReadingInterpretationRepository
        _interpretationRepository;

    public GenerateReadingInterpretationService(
        IReadingRepository readingRepository,
        IReadingInterpreter readingInterpreter,
        IReadingInterpretationRepository interpretationRepository)
    {
        _readingRepository = readingRepository;
        _readingInterpreter = readingInterpreter;
        _interpretationRepository = interpretationRepository;
    }

    public async Task<ReadingInterpretationDetails?> GenerateAsync(
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
         * Construct the interpretation input exclusively from the trusted
         * PostgreSQL snapshot.
         *
         * The browser supplies only readingId.
         */
        var input = new ReadingInterpretationInput(
            ReadingId: reading.Id,
            UserContext: reading.UserContext,
            Spread: new InterpretationSpread(
                Key: reading.SpreadKey,
                Name: reading.SpreadName,
                Description: reading.SpreadDescription,
                IsCustom: reading.IsCustomSpread),
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

        var generated = await _readingInterpreter.InterpretAsync(
            input,
            cancellationToken);

        ValidatePositionReferences(input, generated);

        var persisted = new ReadingInterpretationDetails(
            Id: Guid.NewGuid(),
            ReadingId: reading.Id,
            OverallTheme: generated.OverallTheme,
            PositionInterpretations: generated.PositionInterpretations
                .OrderBy(item => item.PositionOrder)
                .ToArray(),
            Synthesis: generated.Synthesis,
            ReflectionQuestions: generated.ReflectionQuestions
                .ToArray(),
            CreatedAtUtc: DateTimeOffset.UtcNow);

        await _interpretationRepository.SaveAsync(
            persisted,
            cancellationToken);

        return persisted;
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