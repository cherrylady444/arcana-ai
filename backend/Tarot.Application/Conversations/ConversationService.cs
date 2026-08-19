using Tarot.Application.Interpretations;
using Tarot.Application.Readings;

namespace Tarot.Application.Conversations;

public sealed class ConversationService : IConversationService
{
    private const int MaxMessageLength = 4000;
    private const int MaxContextMessages = 25;
    private const int MaxTitleLength = 72;

    private readonly IConversationRepository _conversationRepository;
    private readonly IConversationResponder _conversationResponder;
    private readonly IReadingRepository _readingRepository;
    private readonly IReadingInterpretationRepository _interpretationRepository;

    public ConversationService(
        IConversationRepository conversationRepository,
        IConversationResponder conversationResponder,
        IReadingRepository readingRepository,
        IReadingInterpretationRepository interpretationRepository)
    {
        _conversationRepository = conversationRepository;
        _conversationResponder = conversationResponder;
        _readingRepository = readingRepository;
        _interpretationRepository = interpretationRepository;
    }

    public async Task<ConversationDetails> CreateAsync(
        CreateConversationCommand command,
        CancellationToken cancellationToken = default)
    {
        var normalizedMessage = NormalizeMessage(command.Message);
        var context = await ResolveContextAsync(
            command.ReadingId,
            command.InterpretationId,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var userMessage = new ConversationMessageDetails(
            Id: Guid.NewGuid(),
            Role: ConversationMessageRole.User,
            Content: normalizedMessage,
            CreatedAtUtc: now);

        var assistantContent = await _conversationResponder.RespondAsync(
            new ConversationResponseInput(
                Messages: [userMessage],
                Context: context),
            cancellationToken);

        var assistantMessage = new ConversationMessageDetails(
            Id: Guid.NewGuid(),
            Role: ConversationMessageRole.Assistant,
            Content: NormalizeAssistantMessage(assistantContent),
            CreatedAtUtc: DateTimeOffset.UtcNow);

        var conversation = new ConversationDetails(
            Id: Guid.NewGuid(),
            Title: CreateTitle(normalizedMessage),
            ReadingId: context?.Reading?.Id,
            InterpretationId: context?.Interpretation?.Id,
            Messages: [userMessage, assistantMessage],
            CreatedAtUtc: now,
            UpdatedAtUtc: assistantMessage.CreatedAtUtc);

        await _conversationRepository.SaveAsync(conversation, cancellationToken);

        return conversation;
    }

    public Task<ConversationDetails?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _conversationRepository.GetByIdAsync(id, cancellationToken);
    }

    public Task<PagedConversationsResult> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page));
        }

        if (pageSize is < 1 or > 50)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        return _conversationRepository.GetPageAsync(page, pageSize, cancellationToken);
    }

    public async Task<ConversationDetails?> SendMessageAsync(
        Guid conversationId,
        string message,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(
            conversationId,
            cancellationToken);

        if (conversation is null)
        {
            return null;
        }

        var normalizedMessage = NormalizeMessage(message);
        var context = await ResolveContextAsync(
            conversation.ReadingId,
            conversation.InterpretationId,
            cancellationToken);

        var userMessage = new ConversationMessageDetails(
            Id: Guid.NewGuid(),
            Role: ConversationMessageRole.User,
            Content: normalizedMessage,
            CreatedAtUtc: DateTimeOffset.UtcNow);

        var recentHistory = conversation.Messages
            .TakeLast(MaxContextMessages - 1)
            .Append(userMessage)
            .ToArray();

        var assistantContent = await _conversationResponder.RespondAsync(
            new ConversationResponseInput(
                Messages: recentHistory,
                Context: context),
            cancellationToken);

        var assistantMessage = new ConversationMessageDetails(
            Id: Guid.NewGuid(),
            Role: ConversationMessageRole.Assistant,
            Content: NormalizeAssistantMessage(assistantContent),
            CreatedAtUtc: DateTimeOffset.UtcNow);

        await _conversationRepository.AppendExchangeAsync(
            conversationId,
            userMessage,
            assistantMessage,
            assistantMessage.CreatedAtUtc,
            cancellationToken);

        return await _conversationRepository.GetByIdAsync(
            conversationId,
            cancellationToken);
    }

    private async Task<ConversationAiContext?> ResolveContextAsync(
        Guid? readingId,
        Guid? interpretationId,
        CancellationToken cancellationToken)
    {
        ReadingInterpretationDetails? interpretation = null;
        ReadingDetails? reading = null;

        if (interpretationId.HasValue)
        {
            interpretation = await _interpretationRepository.GetByIdAsync(
                interpretationId.Value,
                cancellationToken);

            if (interpretation is null)
            {
                throw new ConversationContextNotFoundException(
                    $"Interpretation '{interpretationId}' was not found.");
            }

            if (readingId.HasValue && readingId.Value != interpretation.ReadingId)
            {
                throw new ArgumentException(
                    "The supplied reading does not belong to the supplied interpretation.");
            }

            readingId = interpretation.ReadingId;
        }

        if (readingId.HasValue)
        {
            reading = await _readingRepository.GetByIdAsync(
                readingId.Value,
                cancellationToken);

            if (reading is null)
            {
                throw new ConversationContextNotFoundException(
                    $"Reading '{readingId}' was not found.");
            }
        }

        if (reading is null && interpretation is null)
        {
            return null;
        }

        return new ConversationAiContext(reading, interpretation);
    }

    private static string NormalizeMessage(string? message)
    {
        var normalized = message?.Trim() ?? string.Empty;

        if (normalized.Length == 0)
        {
            throw new ArgumentException("A conversation message is required.", nameof(message));
        }

        if (normalized.Length > MaxMessageLength)
        {
            throw new ArgumentException(
                $"A conversation message cannot exceed {MaxMessageLength} characters.",
                nameof(message));
        }

        return normalized;
    }

    private static string NormalizeAssistantMessage(string? message)
    {
        var normalized = message?.Trim() ?? string.Empty;

        if (normalized.Length == 0)
        {
            throw new ConversationResponderResponseException(
                "The AI service returned an empty conversation response.");
        }

        return normalized;
    }

    private static string CreateTitle(string message)
    {
        var compact = string.Join(
            " ",
            message.Split(
                ['\r', '\n', '\t', ' '],
                StringSplitOptions.RemoveEmptyEntries));

        if (compact.Length <= MaxTitleLength)
        {
            return compact;
        }

        return $"{compact[..(MaxTitleLength - 1)].TrimEnd()}…";
    }
}
