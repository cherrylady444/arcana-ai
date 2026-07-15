namespace Tarot.Domain;

/// <summary>
/// Represents a whole tarot spread definition, including its key, name, description, intent, and positions.
/// </summary>
public sealed record SpreadDefinition(
    string Key,
    string Name,
    string ShortDescription,
    string Description,
    string Intent,
    IReadOnlyList<SpreadPositionDefinition> Positions,
    bool IsBuiltIn = true
)
{
    public int CardCount => Positions.Count;
}
