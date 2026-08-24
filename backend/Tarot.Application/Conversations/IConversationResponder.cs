namespace Tarot.Application.Conversations;

public interface IConversationResponder
{
    Task<ConversationResponderResult> RespondAsync(
        ConversationResponseInput input,
        CancellationToken cancellationToken = default);
}
