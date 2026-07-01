namespace Tarot.Application.Readings;

/// <summary>
/// Creates a tarot reading for the specified question.
/// </summary>
/// <param name="Question">The question for which to create a tarot reading.</param>
public sealed record CreateReadingCommand(
    string Question
);