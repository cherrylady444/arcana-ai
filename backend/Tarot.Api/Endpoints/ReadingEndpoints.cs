using Tarot.Api.Contracts;
using Tarot.Application.Interpretations;
using Tarot.Application.Readings;
using Tarot.Domain;

namespace Tarot.Api.Endpoints;

public static class ReadingEndpoints
{
    public static IEndpointRouteBuilder MapReadingEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/readings");

        group.MapPost("", CreateReading);

        group.MapGet("", ListReadings);

        group.MapGet("/{id:guid}", GetReadingById);

        group.MapPost(
            "/{id:guid}/interpretations",
            GenerateReadingInterpretation);

        group.MapGet(
            "/{id:guid}/interpretations",
            ListReadingInterpretations);

        group.MapGet(
            "/{id:guid}/interpretations/{interpretationId:guid}",
            GetReadingInterpretationById);

        return endpoints;
    }

    private static async Task<IResult> CreateReading(
        CreateReadingRequest request,
        IReadingService readingService,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await readingService.CreateReadingAsync(
                new CreateReadingCommand(
                    SpreadKey: request.SpreadKey,
                    UserContext: request.UserContext),
                cancellationToken);

            var response = new ReadingResponse(
                Id: result.Id,
                UserContext: result.UserContext,
                SpreadKey: result.SpreadKey,
                SpreadName: result.SpreadName,
                Cards: result.Cards
                    .Select(ToCreatedReadingCardResponse)
                    .ToList(),
                CreatedAtUtc: result.CreatedAtUtc);

            return Results.Ok(response);
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(new
            {
                error = exception.Message
            });
        }
    }

    private static async Task<IResult> GetReadingById(
        Guid id,
        IGetReadingByIdService getReadingByIdService,
        CancellationToken cancellationToken)
    {
        var reading = await getReadingByIdService.GetByIdAsync(
            id,
            cancellationToken);

        if (reading is null)
        {
            return Results.NotFound(new
            {
                error = $"Reading '{id}' was not found."
            });
        }

        var response = new ReadingResponse(
            Id: reading.Id,
            UserContext: reading.UserContext,
            SpreadKey: reading.SpreadKey,
            SpreadName: reading.SpreadName,
            Cards: reading.Cards
                .Select(ToSavedReadingCardResponse)
                .ToList(),
            CreatedAtUtc: reading.CreatedAtUtc);

        return Results.Ok(response);
    }

    private static async Task<IResult> ListReadings(
        int? page,
        int? pageSize,
        IListReadingsService listReadingsService,
        CancellationToken cancellationToken)
    {
        var requestedPage = page ?? 1;
        var requestedPageSize = pageSize ?? 20;

        try
        {
            var result = await listReadingsService.GetPageAsync(
                requestedPage,
                requestedPageSize,
                cancellationToken);

            return Results.Ok(result);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return Results.BadRequest(new
            {
                error = exception.Message
            });
        }
    }

    /// <summary>
    /// Generates and persists a new interpretation version.
    ///
    /// The caller supplies only the reading ID.
    /// </summary>
    private static async Task<IResult> GenerateReadingInterpretation(
        Guid id,
        IGenerateReadingInterpretationService interpretationService,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(
            "Tarot.Api.ReadingInterpretations");

        try
        {
            var interpretation = await interpretationService.GenerateAsync(
                id,
                cancellationToken);

            if (interpretation is null)
            {
                return Results.NotFound(new
                {
                    error = $"Reading '{id}' was not found."
                });
            }

            var response = ToReadingInterpretationResponse(
                interpretation);

            var location =
                $"/api/readings/{id}/interpretations/{interpretation.Id}";

            return Results.Created(location, response);
        }
        catch (ReadingInterpreterUnavailableException exception)
        {
            logger.LogWarning(
                exception,
                "AI interpretation is unavailable for reading {ReadingId}.",
                id);

            return Results.Json(
                new
                {
                    error =
                        "AI interpretation is temporarily unavailable."
                },
                statusCode:
                    StatusCodes.Status503ServiceUnavailable);
        }
        catch (ReadingInterpreterResponseException exception)
        {
            logger.LogError(
                exception,
                "AI interpretation returned an invalid response for " +
                "reading {ReadingId}.",
                id);

            return Results.Json(
                new
                {
                    error =
                        "AI interpretation provider returned an invalid response."
                },
                statusCode:
                    StatusCodes.Status502BadGateway);
        }
    }

    private static async Task<IResult> GetReadingInterpretationById(
        Guid id,
        Guid interpretationId,
        IGetReadingInterpretationService interpretationService,
        CancellationToken cancellationToken)
    {
        var interpretation = await interpretationService.GetByIdAsync(
            id,
            interpretationId,
            cancellationToken);

        if (interpretation is null)
        {
            return Results.NotFound(new
            {
                error =
                    $"Interpretation '{interpretationId}' was not found " +
                    $"for reading '{id}'."
            });
        }

        return Results.Ok(
            ToReadingInterpretationResponse(interpretation));
    }

    private static async Task<IResult> ListReadingInterpretations(
        Guid id,
        IListReadingInterpretationsService interpretationService,
        CancellationToken cancellationToken)
    {
        var interpretations =
            await interpretationService.GetByReadingIdAsync(
                id,
                cancellationToken);

        if (interpretations is null)
        {
            return Results.NotFound(new
            {
                error = $"Reading '{id}' was not found."
            });
        }

        var response = interpretations
            .Select(ToReadingInterpretationResponse)
            .ToArray();

        return Results.Ok(response);
    }

    private static ReadingInterpretationResponse
        ToReadingInterpretationResponse(
            ReadingInterpretationDetails interpretation)
    {
        return new ReadingInterpretationResponse(
            Id: interpretation.Id,
            ReadingId: interpretation.ReadingId,
            OverallTheme: interpretation.OverallTheme,
            PositionInterpretations: interpretation
                .PositionInterpretations
                .OrderBy(item => item.PositionOrder)
                .Select(item => new PositionInterpretationResponse(
                    PositionOrder: item.PositionOrder,
                    Interpretation: item.Interpretation))
                .ToArray(),
            Synthesis: interpretation.Synthesis,
            ReflectionQuestions: interpretation.ReflectionQuestions
                .ToArray(),
            CreatedAtUtc: interpretation.CreatedAtUtc);
    }

    private static ReadingCardResponse ToCreatedReadingCardResponse(
        ReadingCard readingCard)
    {
        return new ReadingCardResponse(
            PositionOrder: readingCard.Position.Order,
            PositionKey: readingCard.Position.Key,
            PositionTitle: readingCard.Position.Title,
            PositionPrompt: readingCard.Position.Prompt,
            PositionDescription: readingCard.Position.Description,
            CardId: readingCard.Card.Id,
            CardName: readingCard.Card.Name,
            Arcana: readingCard.Card.Arcana,
            Suit: readingCard.Card.Suit,
            Orientation: readingCard.Orientation.ToString(),
            Meaning: readingCard.Meaning);
    }

    private static ReadingCardResponse ToSavedReadingCardResponse(
        ReadingCardDetails readingCard)
    {
        return new ReadingCardResponse(
            PositionOrder: readingCard.PositionOrder,
            PositionKey: readingCard.PositionKey,
            PositionTitle: readingCard.PositionTitle,
            PositionPrompt: readingCard.PositionPrompt,
            PositionDescription: readingCard.PositionDescription,
            CardId: readingCard.CardId,
            CardName: readingCard.CardName,
            Arcana: readingCard.Arcana,
            Suit: readingCard.Suit,
            Orientation: readingCard.Orientation,
            Meaning: readingCard.Meaning);
    }
}