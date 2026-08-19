namespace Tarot.Application.Conversations;

public interface IConversationResponder
{
    Task<string> RespondAsync(
        ConversationResponseInput input,
        CancellationToken cancellationToken = default);
}
