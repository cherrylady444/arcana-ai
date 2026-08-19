namespace Tarot.Application.Interpretations;

public interface IGenerateConversationInterpretationService
{
    Task<ReadingInterpretationDetails?> GenerateAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);
}
