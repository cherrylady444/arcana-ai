using Microsoft.EntityFrameworkCore;
using Tarot.Application.Conversations;
using Tarot.Infrastructure.Persistence.Entities;

namespace Tarot.Infrastructure.Persistence.Repositories;

public sealed class PostgresConversationRepository : IConversationRepository
{
    private readonly TarotDbContext _dbContext;

    public PostgresConversationRepository(TarotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveAsync(
        ConversationDetails conversation,
        CancellationToken cancellationToken = default)
    {
        var entity = new ConversationEntity
        {
            Id = conversation.Id,
            Title = conversation.Title,
            ReadingId = conversation.ReadingId,
            InterpretationId = conversation.InterpretationId,
            CreatedAtUtc = conversation.CreatedAtUtc,
            UpdatedAtUtc = conversation.UpdatedAtUtc,
            Messages = conversation.Messages
                .Select((message, index) => ToEntity(conversation.Id, message, index + 1))
                .ToList()
        };

        _dbContext.Conversations.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ConversationDetails?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Conversations
            .AsNoTracking()
            .Include(conversation => conversation.Messages)
            .SingleOrDefaultAsync(conversation => conversation.Id == id, cancellationToken);

        return entity is null ? null : ToDetails(entity);
    }

    public async Task<PagedConversationsResult> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Conversations.AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);

        var entities = await query
            .Include(conversation => conversation.Messages)
            .OrderByDescending(conversation => conversation.UpdatedAtUtc)
            .ThenByDescending(conversation => conversation.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = entities
            .Select(entity =>
            {
                var lastMessage = entity.Messages
                    .OrderByDescending(message => message.Sequence)
                    .FirstOrDefault();

                return new ConversationSummary(
                    Id: entity.Id,
                    Title: entity.Title,
                    ReadingId: entity.ReadingId,
                    InterpretationId: entity.InterpretationId,
                    LastMessagePreview: CreatePreview(lastMessage?.Content),
                    MessageCount: entity.Messages.Count,
                    CreatedAtUtc: entity.CreatedAtUtc,
                    UpdatedAtUtc: entity.UpdatedAtUtc);
            })
            .ToArray();

        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedConversationsResult(
            Items: items,
            Page: page,
            PageSize: pageSize,
            TotalCount: totalCount,
            TotalPages: totalPages);
    }

    public async Task AppendExchangeAsync(
        Guid conversationId,
        ConversationMessageDetails userMessage,
        ConversationMessageDetails assistantMessage,
        DateTimeOffset updatedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _dbContext.Conversations
            .SingleOrDefaultAsync(item => item.Id == conversationId, cancellationToken);

        if (conversation is null)
        {
            throw new InvalidOperationException(
                $"Conversation '{conversationId}' no longer exists.");
        }

        var lastSequence = await _dbContext.ConversationMessages
            .Where(message => message.ConversationId == conversationId)
            .MaxAsync(message => (int?)message.Sequence, cancellationToken)
            ?? 0;

        _dbContext.ConversationMessages.AddRange(
            ToEntity(conversationId, userMessage, lastSequence + 1),
            ToEntity(conversationId, assistantMessage, lastSequence + 2));

        conversation.UpdatedAtUtc = updatedAtUtc;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AttachReadingAsync(
        Guid conversationId,
        Guid readingId,
        DateTimeOffset updatedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _dbContext.Conversations
            .SingleOrDefaultAsync(item => item.Id == conversationId, cancellationToken);

        if (conversation is null)
        {
            throw new InvalidOperationException(
                $"Conversation '{conversationId}' no longer exists.");
        }

        if (conversation.ReadingId.HasValue)
        {
            throw new InvalidOperationException(
                $"Conversation '{conversationId}' is already linked to a reading.");
        }

        conversation.ReadingId = readingId;
        conversation.UpdatedAtUtc = updatedAtUtc;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static ConversationMessageEntity ToEntity(
        Guid conversationId,
        ConversationMessageDetails message,
        int sequence)
    {
        return new ConversationMessageEntity
        {
            Id = message.Id,
            ConversationId = conversationId,
            Sequence = sequence,
            Role = message.Role == ConversationMessageRole.User ? "user" : "assistant",
            Content = message.Content,
            SuggestedAction = message.SuggestedAction == ConversationSuggestedAction.Reading
                ? "reading"
                : null,
            CreatedAtUtc = message.CreatedAtUtc
        };
    }

    private static ConversationDetails ToDetails(ConversationEntity entity)
    {
        return new ConversationDetails(
            Id: entity.Id,
            Title: entity.Title,
            ReadingId: entity.ReadingId,
            InterpretationId: entity.InterpretationId,
            Messages: entity.Messages
                .OrderBy(message => message.Sequence)
                .Select(message => new ConversationMessageDetails(
                    Id: message.Id,
                    Role: string.Equals(message.Role, "user", StringComparison.OrdinalIgnoreCase)
                        ? ConversationMessageRole.User
                        : ConversationMessageRole.Assistant,
                    Content: message.Content,
                    SuggestedAction: string.Equals(
                        message.SuggestedAction,
                        "reading",
                        StringComparison.OrdinalIgnoreCase)
                            ? ConversationSuggestedAction.Reading
                            : null,
                    CreatedAtUtc: message.CreatedAtUtc))
                .ToArray(),
            CreatedAtUtc: entity.CreatedAtUtc,
            UpdatedAtUtc: entity.UpdatedAtUtc);
    }

    private static string CreatePreview(string? value)
    {
        var normalized = value?.Trim() ?? string.Empty;
        const int maxLength = 140;

        return normalized.Length <= maxLength
            ? normalized
            : $"{normalized[..(maxLength - 1)].TrimEnd()}…";
    }
}
