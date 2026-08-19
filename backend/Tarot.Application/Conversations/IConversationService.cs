namespace Tarot.Application.Conversations;

public interface IConversationService
{
    Task<ConversationDetails> CreateAsync(
        CreateConversationCommand command,
        CancellationToken cancellationToken = default);

    Task<ConversationDetails?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PagedConversationsResult> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ConversationDetails?> SendMessageAsync(
        Guid conversationId,
        string message,
        CancellationToken cancellationToken = default);
}
