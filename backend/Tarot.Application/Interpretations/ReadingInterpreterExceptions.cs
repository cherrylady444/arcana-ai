namespace Tarot.Application.Interpretations;

/// <summary>
/// Raised when the external interpretation service cannot currently be used,
/// for example because of a timeout, network failure, or temporary 5xx error.
/// These allow the API to distinguish:
/// service unavailable → 503
/// invalid provider response → 502
/// </summary>
public sealed class ReadingInterpreterUnavailableException : Exception
{
    public ReadingInterpreterUnavailableException(
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Raised when the interpretation service responds but its result cannot be
/// safely accepted, for example because its JSON or position references are
/// invalid.
/// </summary>
public sealed class ReadingInterpreterResponseException : Exception
{
    public ReadingInterpreterResponseException(
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
    }
}