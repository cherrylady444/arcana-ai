namespace Tarot.Application.Readings;

/// <summary>
/// Creates a tarot reading for the specified question and spread.
/// </summary>
/// <param name="Question">The question for which to create a tarot reading.</param>
/// <param name="SpreadKey">The selected spread key. If omitted, the default spread is used.</param>
public sealed record CreateReadingCommand(
    string? SpreadKey = null,
    string? UserContext = null
);

