using Tarot.Domain;

namespace Tarot.Application.Readings;

/*
For now, it contains:

reading ID
question
3 cards
creation time

Later, it can also contain:

AI interpretation
spread type
user ID
saved database ID
*/
public sealed record CreateReadingResult(
    Guid Id,
    string Question,
    IReadOnlyCollection<TarotCard> Cards,
    DateTimeOffset CreatedAtUtc
);