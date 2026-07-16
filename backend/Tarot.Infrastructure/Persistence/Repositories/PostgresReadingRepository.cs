using Tarot.Application.Readings;
using Tarot.Infrastructure.Persistence.Entities;

namespace Tarot.Infrastructure.Persistence.Repositories;

public sealed class PostgresReadingRepository : IReadingRepository
{
    private readonly TarotDbContext _dbContext;

    public PostgresReadingRepository(TarotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveAsync(
        CreateReadingResult reading,
        CancellationToken cancellationToken = default)
    {
        var entity = new ReadingEntity
        {
            Id = reading.Id,
            UserContext = reading.UserContext,
            SpreadKey = reading.SpreadKey,
            SpreadName = reading.SpreadName,
            CreatedAtUtc = reading.CreatedAtUtc,
            Cards = reading.Cards
                .OrderBy(card => card.Position.Order)
                .Select(card => new ReadingCardEntity
                {
                    Id = Guid.NewGuid(),
                    ReadingId = reading.Id,

                    PositionOrder = card.Position.Order,
                    PositionKey = card.Position.Key,
                    PositionTitle = card.Position.Title,
                    PositionPrompt = card.Position.Prompt,
                    PositionDescription = card.Position.Description,

                    CardId = card.Card.Id,
                    CardName = card.Card.Name,
                    Arcana = card.Card.Arcana,
                    Suit = card.Card.Suit,

                    Orientation = card.Orientation.ToString(),
                    Meaning = card.Meaning
                })
                .ToList()
        };

        _dbContext.Readings.Add(entity);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}