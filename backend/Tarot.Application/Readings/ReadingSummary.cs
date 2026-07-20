namespace Tarot.Application.Readings;

public sealed record ReadingSummary(
    Guid Id,
    string? UserContext,
    string SpreadKey,
    string SpreadName,
    int CardCount,
    DateTimeOffset CreatedAtUtc
);