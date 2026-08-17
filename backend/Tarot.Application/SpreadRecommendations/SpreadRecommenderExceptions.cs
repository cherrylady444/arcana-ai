namespace Tarot.Application.SpreadRecommendations;

public sealed class SpreadRecommenderUnavailableException : Exception
{
    public SpreadRecommenderUnavailableException(
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

public sealed class SpreadRecommenderResponseException : Exception
{
    public SpreadRecommenderResponseException(
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
