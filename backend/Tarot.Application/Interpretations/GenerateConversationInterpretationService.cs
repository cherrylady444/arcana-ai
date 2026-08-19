using Tarot.Application.Conversations;
using Tarot.Application.Readings;

namespace Tarot.Application.Interpretations;

public sealed class GenerateConversationInterpretationService
    : IGenerateConversationInterpretationService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IReadingRepository _readingRepository;
    private readonly IReadingInterpreter _readingInterpreter;
    private readonly IReadingInterpretationRepository _interpretationRepository;

    public GenerateConversationInterpretationService(
        IConversationRepository conversationRepository,
        IReadingRepository readingRepository,
        IReadingInterpreter readingInterpreter,
        IReadingInterpretationRepository interpretationRepository)
    {
        _conversationRepository = conversationRepository;
        _readingRepository = readingRepository;
        _readingInterpreter = readingInterpreter;
        _interpretationRepository = interpretationRepository;
    }

    public async Task<ReadingInterpretationDetails?> GenerateAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(
            conversationId,
            cancellationToken);

        if (conversation is null)
        {
            return null;
        }

        if (!conversation.ReadingId.HasValue)
        {
            throw new ArgumentException(
                "This conversation is not linked to a reading. Start from a reading before creating a structured interpretation.");
        }

        var reading = await _readingRepository.GetByIdAsync(
            conversation.ReadingId.Value,
            cancellationToken);

        if (reading is null)
        {
            throw new ConversationContextNotFoundException(
                $"Reading '{conversation.ReadingId}' was not found.");
        }

        var input = ReadingInterpretationInputFactory.Create(
            reading,
            conversation.Messages);

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
            ReflectionQuestions: generated.ReflectionQuestions.ToArray(),
            CreatedAtUtc: DateTimeOffset.UtcNow,
            SourceConversationId: conversation.Id);

        await _interpretationRepository.SaveAsync(persisted, cancellationToken);

        return persisted;
    }
}
