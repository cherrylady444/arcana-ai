using Tarot.Application.Spreads;

namespace Tarot.Application.Readings;

/// <summary>
/// Creates a tarot reading from either one predefined spread key or one
/// validated custom spread definition.
/// </summary>
public sealed record CreateReadingCommand(
    string? SpreadKey = null,
    string? UserContext = null,
    CustomSpreadDefinition? CustomSpread = null
);
