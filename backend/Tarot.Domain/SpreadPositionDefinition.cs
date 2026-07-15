namespace Tarot.Domain;

/// <summary>
/// Represents a single card position inside that spread.
/// </summary>
public sealed record SpreadPositionDefinition(
    int Order,
    string Key,
    string Title,
    string Prompt,
    string Description
);
