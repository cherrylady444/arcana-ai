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

        var input = ReadingInterpretationInputFactory.Create(reading);

        var generated = await _readingInterpreter.InterpretAsync(
            input,
            cancellationToken);

        ReadingInterpretationInputFactory.ValidatePositionReferences(input, generated);

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
}
