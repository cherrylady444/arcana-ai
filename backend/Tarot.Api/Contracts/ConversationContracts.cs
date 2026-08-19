namespace Tarot.Api.Contracts;

public sealed record CreateConversationRequest(
    string Message,
    Guid? ReadingId = null,
    Guid? InterpretationId = null
);

public sealed record SendConversationMessageRequest(string Message);

public sealed record ConversationMessageResponse(
    Guid Id,
    string Role,
    string Content,
    DateTimeOffset CreatedAtUtc
);

public sealed record ConversationResponse(
    Guid Id,
    string Title,
    Guid? ReadingId,
    Guid? InterpretationId,
    IReadOnlyCollection<ConversationMessageResponse> Messages,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc
);

public sealed record ConversationSummaryResponse(
    Guid Id,
    string Title,
    Guid? ReadingId,
    Guid? InterpretationId,
    string LastMessagePreview,
    int MessageCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc
);

public sealed record PagedConversationsResponse(
    IReadOnlyCollection<ConversationSummaryResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);
