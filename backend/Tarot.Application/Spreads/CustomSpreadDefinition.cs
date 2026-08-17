namespace Tarot.Application.Spreads;

/// <summary>
/// Untrusted custom spread input accepted from either the AI recommendation
/// flow or the frontend when a user starts the recommended reading.
/// </summary>
public sealed record CustomSpreadDefinition(
    string Name,
    string Description,
    IReadOnlyCollection<CustomSpreadPositionDefinition> Positions
);

public sealed record CustomSpreadPositionDefinition(
    int Order,
    string Title,
    string Prompt,
    string Description
);