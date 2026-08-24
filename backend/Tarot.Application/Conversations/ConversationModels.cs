using Tarot.Application.Interpretations;
using Tarot.Application.Readings;
using Tarot.Application.Spreads;

namespace Tarot.Application.Conversations;

public enum ConversationMessageRole
{
    User,
    Assistant
}

public enum ConversationSuggestedAction
{
    Reading
}

public sealed record ConversationMessageDetails(
    Guid Id,
    ConversationMessageRole Role,
    string Content,
    DateTimeOffset CreatedAtUtc,
    ConversationSuggestedAction? SuggestedAction = null
);

public sealed record ConversationDetails(
    Guid Id,
    string Title,
    Guid? ReadingId,
    Guid? InterpretationId,
    IReadOnlyCollection<ConversationMessageDetails> Messages,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc
);

public sealed record ConversationSummary(
    Guid Id,
    string Title,
    Guid? ReadingId,
    Guid? InterpretationId,
    string LastMessagePreview,
    int MessageCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc
);

public sealed record PagedConversationsResult(
    IReadOnlyCollection<ConversationSummary> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);

public sealed record CreateConversationCommand(
    string Message,
    Guid? ReadingId = null,
    Guid? InterpretationId = null
);

public sealed record CreateConversationReadingCommand(
    string? SpreadKey,
    CustomSpreadDefinition? CustomSpread
);

public sealed record ConversationAiContext(
    ReadingDetails? Reading,
    ReadingInterpretationDetails? Interpretation
);

public sealed record ConversationResponseInput(
    IReadOnlyCollection<ConversationMessageDetails> Messages,
    ConversationAiContext? Context
);

public sealed record ConversationResponderResult(
    string Message,
    ConversationSuggestedAction? SuggestedAction
);
