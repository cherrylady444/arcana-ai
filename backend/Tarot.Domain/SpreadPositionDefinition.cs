namespace Tarot.Domain;

public sealed record SpreadPositionDefinition(
    int Order,
    string Key,
    string Title,
    string Prompt,
    string Description
);
