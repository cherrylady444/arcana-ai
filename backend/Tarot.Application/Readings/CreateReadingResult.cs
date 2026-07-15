using Tarot.Domain;

namespace Tarot.Application.Readings;

/*
Later, it can also contain:

AI interpretation
spread type
user ID
saved database ID
*/
public sealed record CreateReadingResult(
    Guid Id,
    string? UserContext,
    string SpreadKey,
    string SpreadName,
    IReadOnlyCollection<ReadingCard> Cards,
    DateTimeOffset CreatedAtUtc
);
