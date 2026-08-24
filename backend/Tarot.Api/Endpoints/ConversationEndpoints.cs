using Tarot.Api.Contracts;
using Tarot.Application.Conversations;
using Tarot.Application.Interpretations;
using Tarot.Application.SpreadRecommendations;
using Tarot.Application.Spreads;

namespace Tarot.Api.Endpoints;

public static class ConversationEndpoints
{
    public static IEndpointRouteBuilder MapConversationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/conversations");

        group.MapPost("", CreateConversation);
        group.MapGet("", ListConversations);
        group.MapGet("/{id:guid}", GetConversationById);
        group.MapPost("/{id:guid}/messages", SendMessage);
        group.MapPost("/{id:guid}/reading-recommendation", RecommendReading);
        group.MapPost("/{id:guid}/readings", CreateReading);
        group.MapPost("/{id:guid}/interpretations", GenerateInterpretation);

        return endpoints;
    }

    private static async Task<IResult> CreateConversation(
        CreateConversationRequest request,
        IConversationService conversationService,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("ConversationEndpoints");

        try
        {
            var conversation = await conversationService.CreateAsync(
                new CreateConversationCommand(
                    Message: request.Message,
                    ReadingId: request.ReadingId,
                    InterpretationId: request.InterpretationId),
                cancellationToken);

            return Results.Created(
                $"/api/conversations/{conversation.Id}",
                ToResponse(conversation));
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(new { error = exception.Message });
        }
        catch (ConversationContextNotFoundException exception)
        {
            return Results.NotFound(new { error = exception.Message });
        }
        catch (ConversationResponderUnavailableException exception)
        {
            logger.LogError(exception, "AI conversation provider is unavailable.");
            return Results.Json(
                new { error = "Arcana is temporarily unavailable. Please try again in a moment." },
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (ConversationResponderResponseException exception)
        {
            logger.LogError(exception, "AI conversation provider returned an invalid response.");
            return Results.Json(
                new { error = "Arcana could not complete that response. Please try again." },
                statusCode: StatusCodes.Status502BadGateway);
        }
    }

    private static async Task<IResult> ListConversations(
        int? page,
        int? pageSize,
        IConversationService conversationService,
        CancellationToken cancellationToken)
    {
        var normalizedPage = page ?? 1;
        var normalizedPageSize = pageSize ?? 12;

        try
        {
            var result = await conversationService.GetPageAsync(
                normalizedPage,
                normalizedPageSize,
                cancellationToken);

            return Results.Ok(new PagedConversationsResponse(
                Items: result.Items.Select(ToSummaryResponse).ToArray(),
                Page: result.Page,
                PageSize: result.PageSize,
                TotalCount: result.TotalCount,
                TotalPages: result.TotalPages));
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return Results.BadRequest(new { error = exception.Message });
        }
    }

    private static async Task<IResult> GetConversationById(
        Guid id,
        IConversationService conversationService,
        CancellationToken cancellationToken)
    {
        var conversation = await conversationService.GetByIdAsync(id, cancellationToken);

        return conversation is null
            ? Results.NotFound(new { error = $"Conversation '{id}' was not found." })
            : Results.Ok(ToResponse(conversation));
    }

    private static async Task<IResult> SendMessage(
        Guid id,
        SendConversationMessageRequest request,
        IConversationService conversationService,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("ConversationEndpoints");

        try
        {
            var conversation = await conversationService.SendMessageAsync(
                id,
                request.Message,
                cancellationToken);

            return conversation is null
                ? Results.NotFound(new { error = $"Conversation '{id}' was not found." })
                : Results.Ok(ToResponse(conversation));
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(new { error = exception.Message });
        }
        catch (ConversationContextNotFoundException exception)
        {
            return Results.NotFound(new { error = exception.Message });
        }
        catch (ConversationResponderUnavailableException exception)
        {
            logger.LogError(exception, "AI conversation provider is unavailable.");
            return Results.Json(
                new { error = "Arcana is temporarily unavailable. Please try again in a moment." },
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (ConversationResponderResponseException exception)
        {
            logger.LogError(exception, "AI conversation provider returned an invalid response.");
            return Results.Json(
                new { error = "Arcana could not complete that response. Please try again." },
                statusCode: StatusCodes.Status502BadGateway);
        }
    }

    private static async Task<IResult> RecommendReading(
        Guid id,
        IConversationService conversationService,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("ConversationEndpoints");

        try
        {
            var recommendation = await conversationService.RecommendReadingAsync(
                id,
                cancellationToken);

            return recommendation is null
                ? Results.NotFound(new { error = $"Conversation '{id}' was not found." })
                : Results.Ok(ToSpreadRecommendationResponse(recommendation));
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(new { error = exception.Message });
        }
        catch (SpreadRecommenderUnavailableException exception)
        {
            logger.LogWarning(
                exception,
                "AI spread recommendation is temporarily unavailable for conversation {ConversationId}.",
                id);

            return Results.Json(
                new { error = "Arcana is temporarily unavailable. Please try again in a moment." },
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (SpreadRecommenderResponseException exception)
        {
            logger.LogError(
                exception,
                "AI spread recommendation returned an invalid response for conversation {ConversationId}.",
                id);

            return Results.Json(
                new { error = "Arcana could not complete the spread recommendation. Please try again." },
                statusCode: StatusCodes.Status502BadGateway);
        }
    }

    private static async Task<IResult> CreateReading(
        Guid id,
        CreateConversationReadingRequest request,
        IConversationService conversationService,
        CancellationToken cancellationToken)
    {
        try
        {
            var reading = await conversationService.CreateReadingAsync(
                id,
                new CreateConversationReadingCommand(
                    SpreadKey: request.SpreadKey,
                    CustomSpread: request.CustomSpread is null
                        ? null
                        : new CustomSpreadDefinition(
                            Name: request.CustomSpread.Name,
                            Description: request.CustomSpread.Description,
                            Positions: request.CustomSpread.Positions
                                .Select(position => new CustomSpreadPositionDefinition(
                                    position.Order,
                                    position.Title,
                                    position.Prompt,
                                    position.Description))
                                .ToArray())),
                cancellationToken);

            if (reading is null)
            {
                return Results.NotFound(new { error = $"Conversation '{id}' was not found." });
            }

            return Results.Created(
                $"/api/readings/{reading.Id}",
                new ReadingResponse(
                    Id: reading.Id,
                    UserContext: reading.UserContext,
                    SpreadKey: reading.SpreadKey,
                    SpreadName: reading.SpreadName,
                    SpreadDescription: reading.SpreadDescription,
                    IsCustomSpread: reading.IsCustomSpread,
                    SourceConversationId: reading.SourceConversationId,
                    Cards: reading.Cards
                        .Select(card => new ReadingCardResponse(
                            PositionOrder: card.Position.Order,
                            PositionKey: card.Position.Key,
                            PositionTitle: card.Position.Title,
                            PositionPrompt: card.Position.Prompt,
                            PositionDescription: card.Position.Description,
                            CardId: card.Card.Id,
                            CardName: card.Card.Name,
                            Arcana: card.Card.Arcana,
                            Suit: card.Card.Suit,
                            Orientation: card.Orientation.ToString(),
                            Meaning: card.Meaning))
                        .ToArray(),
                    CreatedAtUtc: reading.CreatedAtUtc));
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(new { error = exception.Message });
        }
    }

    private static async Task<IResult> GenerateInterpretation(
        Guid id,
        IGenerateConversationInterpretationService interpretationService,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("ConversationEndpoints");

        try
        {
            var interpretation = await interpretationService.GenerateAsync(
                id,
                cancellationToken);

            if (interpretation is null)
            {
                return Results.NotFound(new { error = $"Conversation '{id}' was not found." });
            }

            return Results.Created(
                $"/api/interpretations/{interpretation.Id}",
                ReadingInterpretationResponseMapper.ToResponse(interpretation));
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(new { error = exception.Message });
        }
        catch (ConversationContextNotFoundException exception)
        {
            return Results.NotFound(new { error = exception.Message });
        }
        catch (ReadingInterpreterUnavailableException exception)
        {
            logger.LogWarning(
                exception,
                "AI interpretation is unavailable for conversation {ConversationId}.",
                id);

            return Results.Json(
                new { error = "AI interpretation is temporarily unavailable." },
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (ReadingInterpreterResponseException exception)
        {
            logger.LogError(
                exception,
                "AI interpretation returned an invalid response for conversation {ConversationId}.",
                id);

            return Results.Json(
                new { error = "AI interpretation provider returned an invalid response." },
                statusCode: StatusCodes.Status502BadGateway);
        }
    }

    private static ConversationResponse ToResponse(ConversationDetails conversation)
    {
        return new ConversationResponse(
            Id: conversation.Id,
            Title: conversation.Title,
            ReadingId: conversation.ReadingId,
            InterpretationId: conversation.InterpretationId,
            Messages: conversation.Messages
                .Select(message => new ConversationMessageResponse(
                    Id: message.Id,
                    Role: message.Role == ConversationMessageRole.User ? "user" : "assistant",
                    Content: message.Content,
                    SuggestedAction: message.SuggestedAction == ConversationSuggestedAction.Reading
                        ? "reading"
                        : null,
                    CreatedAtUtc: message.CreatedAtUtc))
                .ToArray(),
            CreatedAtUtc: conversation.CreatedAtUtc,
            UpdatedAtUtc: conversation.UpdatedAtUtc);
    }

    private static ConversationSummaryResponse ToSummaryResponse(ConversationSummary conversation)
    {
        return new ConversationSummaryResponse(
            Id: conversation.Id,
            Title: conversation.Title,
            ReadingId: conversation.ReadingId,
            InterpretationId: conversation.InterpretationId,
            LastMessagePreview: conversation.LastMessagePreview,
            MessageCount: conversation.MessageCount,
            CreatedAtUtc: conversation.CreatedAtUtc,
            UpdatedAtUtc: conversation.UpdatedAtUtc);
    }

    private static SpreadRecommendationResponse ToSpreadRecommendationResponse(
        SpreadRecommendation recommendation)
    {
        return new SpreadRecommendationResponse(
            RecommendationType: recommendation.RecommendationType,
            Reason: recommendation.Reason,
            Spread: new RecommendedSpreadResponse(
                Key: recommendation.Spread.Key,
                Name: recommendation.Spread.Name,
                Description: recommendation.Spread.Description,
                IsCustom: recommendation.Spread.IsCustom,
                CardCount: recommendation.Spread.CardCount,
                Positions: recommendation.Spread.Positions
                    .OrderBy(position => position.Order)
                    .Select(position => new RecommendedSpreadPositionResponse(
                        position.Order,
                        position.Key,
                        position.Title,
                        position.Prompt,
                        position.Description))
                    .ToArray()));
    }
}
