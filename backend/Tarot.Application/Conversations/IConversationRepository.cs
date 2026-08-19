namespace Tarot.Application.Conversations;

public interface IConversationRepository
{
    Task SaveAsync(
        ConversationDetails conversation,
        CancellationToken cancellationToken = default);

    Task<ConversationDetails?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PagedConversationsResult> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AppendExchangeAsync(
        Guid conversationId,
        ConversationMessageDetails userMessage,
        ConversationMessageDetails assistantMessage,
        DateTimeOffset updatedAtUtc,
        CancellationToken cancellationToken = default);
}
