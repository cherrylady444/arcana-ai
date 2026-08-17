using Tarot.Domain;

namespace Tarot.Application.Readings;

public sealed record CreateReadingResult(
    Guid Id,
    string? UserContext,
    string SpreadKey,
    string SpreadName,
    IReadOnlyCollection<ReadingCard> Cards,
    DateTimeOffset CreatedAtUtc,
    string SpreadDescription = "",
    bool IsCustomSpread = false
);
