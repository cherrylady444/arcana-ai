namespace Tarot.Application.Conversations;

public sealed class ConversationContextNotFoundException : Exception
{
    public ConversationContextNotFoundException(string message)
        : base(message)
    {
    }
}

public sealed class ConversationResponderUnavailableException : Exception
{
    public ConversationResponderUnavailableException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

public sealed class ConversationResponderResponseException : Exception
{
    public ConversationResponderResponseException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
