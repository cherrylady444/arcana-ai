namespace Tarot.Domain;

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
