namespace Tarot.Infrastructure.AI;

/// <summary>
/// These records are internal because they represent the private contract between:
/// ASP.NET Infrastructure
///         ↕
/// Python FastAPI
/// They are not public frontend contracts.
/// </summary>
internal sealed record PythonInterpretationRequest(
    Guid ReadingId,
    string? UserContext,
    PythonInterpretationSpread Spread,
    IReadOnlyCollection<PythonInterpretationPlacement> Placements,
    IReadOnlyCollection<PythonInterpretationConversationMessage> ConversationContext
);

internal sealed record PythonInterpretationConversationMessage(
    string Role,
    string Content
);

internal sealed record PythonInterpretationSpread(
    string Key,
    string Name,
    string Description,
    bool IsCustom
);

internal sealed record PythonInterpretationPlacement(
    int PositionOrder,
    string PositionKey,
    string PositionTitle,
    string PositionPrompt,
    string PositionDescription,
    Guid CardId,
    string CardName,
    string Arcana,
    string? Suit,
    string Orientation,
    string Meaning
);

internal sealed record PythonInterpretationResponse(
    string? OverallTheme,
    IReadOnlyCollection<PythonPositionInterpretationResponse>?
        PositionInterpretations,
    string? Synthesis,
    IReadOnlyCollection<string>? ReflectionQuestions
);

internal sealed record PythonPositionInterpretationResponse(
    int PositionOrder,
    string? Interpretation
);