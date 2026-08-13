namespace Tarot.Api.Contracts;

/// <summary>
/// Interpretation generated for an existing persisted reading.
/// </summary>
public sealed record ReadingInterpretationResponse(
    Guid Id,
    Guid ReadingId,
    string OverallTheme,
    IReadOnlyCollection<PositionInterpretationResponse>
        PositionInterpretations,
    string Synthesis,
    IReadOnlyCollection<string> ReflectionQuestions,
    DateTimeOffset CreatedAtUtc
);
/// <summary>
/// AI interpretation associated with one existing spread position.
/// </summary>
public sealed record PositionInterpretationResponse(
    int PositionOrder,
    string Interpretation
);