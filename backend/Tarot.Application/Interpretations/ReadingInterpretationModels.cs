namespace Tarot.Application.Interpretations;

/// <summary>
/// Trusted reading data supplied to an interpretation provider.
///
/// This input is constructed by the ASP.NET application from a reading loaded
/// from PostgreSQL. It is never constructed authoritatively by the browser.
/// </summary>
public sealed record ReadingInterpretationInput(
    Guid ReadingId,
    string? UserContext,
    InterpretationSpread Spread,
    IReadOnlyCollection<InterpretationPlacement> Placements
);

/// <summary>
/// Identifies the spread used by the persisted reading.
/// </summary>
public sealed record InterpretationSpread(
    string Key,
    string Name,
    string Description,
    bool IsCustom
);

/// <summary>
/// Represents one immutable persisted relationship between a spread position
/// and a drawn tarot card.
/// </summary>
public sealed record InterpretationPlacement(
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

/// <summary>
/// Structured interpretation returned by the AI provider.
/// </summary>
public sealed record ReadingInterpretation(
    string OverallTheme,
    IReadOnlyCollection<PositionInterpretation> PositionInterpretations,
    string Synthesis,
    IReadOnlyCollection<string> ReflectionQuestions
);

/// <summary>
/// AI interpretation associated with one immutable spread position.
///
/// Card information is intentionally absent. ASP.NET already owns those facts.
/// </summary>
public sealed record PositionInterpretation(
    int PositionOrder,
    string Interpretation
);

/// <summary>
/// A generated interpretation after it has been assigned an ID
/// and persisted as an immutable application resource.
/// </summary>
public sealed record ReadingInterpretationDetails(
    Guid Id,
    Guid ReadingId,
    string OverallTheme,
    IReadOnlyCollection<PositionInterpretation> PositionInterpretations,
    string Synthesis,
    IReadOnlyCollection<string> ReflectionQuestions,
    DateTimeOffset CreatedAtUtc
);