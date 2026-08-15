using Microsoft.EntityFrameworkCore;
using Tarot.Application.Interpretations;
using Tarot.Infrastructure.Persistence.Entities;

namespace Tarot.Infrastructure.Persistence.Repositories;

public sealed class PostgresReadingInterpretationRepository
    : IReadingInterpretationRepository
{
    private readonly TarotDbContext _dbContext;

    public PostgresReadingInterpretationRepository(
        TarotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveAsync(
        ReadingInterpretationDetails interpretation,
        CancellationToken cancellationToken = default)
    {
        var entity = new ReadingInterpretationEntity
        {
            Id = interpretation.Id,
            ReadingId = interpretation.ReadingId,
            OverallTheme = interpretation.OverallTheme,
            Synthesis = interpretation.Synthesis,
            CreatedAtUtc = interpretation.CreatedAtUtc,

            PositionInterpretations = interpretation
                .PositionInterpretations
                .OrderBy(item => item.PositionOrder)
                .Select(item =>
                    new ReadingPositionInterpretationEntity
                    {
                        Id = Guid.NewGuid(),
                        ReadingInterpretationId = interpretation.Id,
                        PositionOrder = item.PositionOrder,
                        Interpretation = item.Interpretation
                    })
                .ToList(),

            ReflectionQuestions = interpretation.ReflectionQuestions
                .Select((question, index) =>
                    new ReadingInterpretationReflectionQuestionEntity
                    {
                        Id = Guid.NewGuid(),
                        ReadingInterpretationId = interpretation.Id,
                        QuestionOrder = index + 1,
                        Question = question
                    })
                .ToList()
        };

        _dbContext.ReadingInterpretations.Add(entity);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ReadingInterpretationDetails?> GetByIdAsync(
        Guid interpretationId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ReadingInterpretations
            .AsNoTracking()
            .AsSplitQuery()
            .Include(
                interpretation =>
                    interpretation.PositionInterpretations)
            .Include(
                interpretation =>
                    interpretation.ReflectionQuestions)
            .SingleOrDefaultAsync(
                interpretation =>
                    interpretation.Id == interpretationId,
                cancellationToken);

        return entity is null
            ? null
            : ToDetails(entity);
    }

    public async Task<ReadingInterpretationDetails?> GetByIdAsync(
        Guid readingId,
        Guid interpretationId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ReadingInterpretations
            .AsNoTracking()
            .AsSplitQuery()
            .Include(
                interpretation =>
                    interpretation.PositionInterpretations)
            .Include(
                interpretation =>
                    interpretation.ReflectionQuestions)
            .SingleOrDefaultAsync(
                interpretation =>
                    interpretation.Id == interpretationId
                    && interpretation.ReadingId == readingId,
                cancellationToken);

        return entity is null
            ? null
            : ToDetails(entity);
    }

    public async Task<IReadOnlyCollection<ReadingInterpretationDetails>>
        GetByReadingIdAsync(
            Guid readingId,
            CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.ReadingInterpretations
            .AsNoTracking()
            .AsSplitQuery()
            .Include(
                interpretation =>
                    interpretation.PositionInterpretations)
            .Include(
                interpretation =>
                    interpretation.ReflectionQuestions)
            .Where(
                interpretation =>
                    interpretation.ReadingId == readingId)
            .OrderByDescending(
                interpretation =>
                    interpretation.CreatedAtUtc)
            .ThenByDescending(
                interpretation =>
                    interpretation.Id)
            .ToListAsync(cancellationToken);

        return entities
            .Select(ToDetails)
            .ToArray();
    }

    private static ReadingInterpretationDetails ToDetails(
        ReadingInterpretationEntity entity)
    {
        return new ReadingInterpretationDetails(
            Id: entity.Id,
            ReadingId: entity.ReadingId,
            OverallTheme: entity.OverallTheme,
            PositionInterpretations: entity.PositionInterpretations
                .OrderBy(item => item.PositionOrder)
                .Select(item => new PositionInterpretation(
                    PositionOrder: item.PositionOrder,
                    Interpretation: item.Interpretation))
                .ToArray(),
            Synthesis: entity.Synthesis,
            ReflectionQuestions: entity.ReflectionQuestions
                .OrderBy(question => question.QuestionOrder)
                .Select(question => question.Question)
                .ToArray(),
            CreatedAtUtc: entity.CreatedAtUtc);
    }
}