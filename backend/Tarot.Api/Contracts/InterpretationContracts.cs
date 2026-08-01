namespace Tarot.Api.Contracts;

/// <summary>
/// Interpretation generated for an existing persisted reading.
/// </summary>
public sealed record ReadingInterpretationResponse(
    string OverallTheme,
    IReadOnlyCollection<PositionInterpretationResponse>
        PositionInterpretations,
    string Synthesis,
    IReadOnlyCollection<string> ReflectionQuestions
);

/// <summary>
/// AI interpretation associated with one existing spread position.
/// </summary>
public sealed record PositionInterpretationResponse(
    int PositionOrder,
    string Interpretation
);