namespace Tarot.Infrastructure.AI;

internal sealed record PythonConversationMessage(
    string Role,
    string Content
);

internal sealed record PythonConversationReadingCard(
    int PositionOrder,
    string PositionTitle,
    string PositionPrompt,
    string PositionDescription,
    string CardName,
    string Arcana,
    string? Suit,
    string Orientation,
    string Meaning
);

internal sealed record PythonConversationReadingContext(
    string Id,
    string? UserContext,
    string SpreadName,
    string SpreadDescription,
    bool IsCustomSpread,
    IReadOnlyCollection<PythonConversationReadingCard> Cards
);

internal sealed record PythonConversationPositionInterpretation(
    int PositionOrder,
    string Interpretation
);

internal sealed record PythonConversationInterpretationContext(
    string Id,
    string OverallTheme,
    IReadOnlyCollection<PythonConversationPositionInterpretation> PositionInterpretations,
    string Synthesis,
    IReadOnlyCollection<string> ReflectionQuestions
);

internal sealed record PythonConversationContext(
    PythonConversationReadingContext? Reading,
    PythonConversationInterpretationContext? Interpretation
);

internal sealed record PythonConversationRequest(
    IReadOnlyCollection<PythonConversationMessage> Messages,
    PythonConversationContext? Context
);

internal sealed record PythonConversationResponse(string Message);
