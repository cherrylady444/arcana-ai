using Tarot.Application.Readings;
using Tarot.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

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

    public async Task<ReadingDetails?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Readings
            .AsNoTracking() // We don't need to track the entity since we are only reading it
            .Include(reading => reading.Cards)
            
            // It returns:
            // one reading when the ID exists;
            // null when it does not exist.
            .SingleOrDefaultAsync(
                reading => reading.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        return new ReadingDetails(
            Id: entity.Id,
            UserContext: entity.UserContext,
            SpreadKey: entity.SpreadKey,
            SpreadName: entity.SpreadName,
            Cards: entity.Cards
                .OrderBy(card => card.PositionOrder)
                .Select(card => new ReadingCardDetails(
                    PositionOrder: card.PositionOrder,
                    PositionKey: card.PositionKey,
                    PositionTitle: card.PositionTitle,
                    PositionPrompt: card.PositionPrompt,
                    PositionDescription: card.PositionDescription,
                    CardId: card.CardId,
                    CardName: card.CardName,
                    Arcana: card.Arcana,
                    Suit: card.Suit,
                    Orientation: card.Orientation,
                    Meaning: card.Meaning
                ))
                .ToList(),
            CreatedAtUtc: entity.CreatedAtUtc
        );
    }

    public async Task<PagedReadingsResult> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Readings
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(reading => reading.CreatedAtUtc)
            // provides deterministic ordering when two readings have the same timestamp.
            .ThenByDescending(reading => reading.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(reading => new ReadingSummary(
                reading.Id,
                reading.UserContext,
                reading.SpreadKey,
                reading.SpreadName,
                reading.Cards.Count,
                reading.CreatedAtUtc
            ))
            .ToListAsync(cancellationToken);

        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedReadingsResult(
            Items: items,
            Page: page,
            PageSize: pageSize,
            TotalCount: totalCount,
            TotalPages: totalPages
        );
    }
}