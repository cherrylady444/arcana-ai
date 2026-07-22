using Tarot.Api.Contracts;
using Tarot.Application.Readings;
using Tarot.Domain;

namespace Tarot.Api.Endpoints;

/// <summary>
/// Contains all Minimal API endpoints related to tarot readings.
/// </summary>
public static class ReadingEndpoints
{
    /// <summary>
    /// Registers the reading endpoints under the common
    /// /api/readings route prefix.
    /// </summary>
    public static IEndpointRouteBuilder MapReadingEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/readings");

        // POST /api/readings
        group.MapPost("", CreateReading);

        // GET /api/readings?page=1&pageSize=20
        group.MapGet("", ListReadings);

        // GET /api/readings/{id}
        group.MapGet("/{id:guid}", GetReadingById);

        return endpoints;
    }

    /// <summary>
    /// Creates a new tarot reading.
    ///
    /// ASP.NET Core reads CreateReadingRequest from the JSON request body and
    /// obtains IReadingService from dependency injection.
    /// </summary>
    private static async Task<IResult> CreateReading(
        CreateReadingRequest request,
        IReadingService readingService,
        CancellationToken cancellationToken)
    {
        try
        {
            /*
             * Convert the HTTP request model into an Application-layer command.
             *
             * The endpoint does not draw cards itself. That responsibility
             * remains inside CreateReadingService.
             */
            var result = await readingService.CreateReadingAsync(
                new CreateReadingCommand(
                    SpreadKey: request.SpreadKey,
                    UserContext: request.UserContext),
                cancellationToken);

            /*
             * Convert the Application result into the response shape exposed
             * publicly by the API.
             */
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
            /*
             * CreateReadingService throws ArgumentException for invalid input,
             * such as an unknown spread key.
             */
            return Results.BadRequest(new
            {
                error = exception.Message
            });
        }
    }

    /// <summary>
    /// Retrieves one previously saved reading by its unique identifier.
    /// </summary>
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

        /*
         * A reading loaded from PostgreSQL contains ReadingCardDetails objects,
         * which differ slightly from the ReadingCard objects produced while
         * creating a new reading.
         */
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

    /// <summary>
    /// Retrieves a paginated list of saved readings.
    ///
    /// When page or pageSize is not supplied, the endpoint uses the default
    /// values page = 1 and pageSize = 20.
    /// </summary>
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

            /*
             * For now, the paginated Application result is returned directly.
             *
             * You can introduce a separate API pagination contract later if
             * the frontend needs a different response shape.
             */
            return Results.Ok(result);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            /*
             * ListReadingsService validates the page and page-size limits.
             */
            return Results.BadRequest(new
            {
                error = exception.Message
            });
        }
    }

    /// <summary>
    /// Converts a newly drawn card into the API response model.
    ///
    /// ReadingCard contains complete Domain objects:
    /// - Position
    /// - Card
    /// - Orientation
    /// - selected Meaning
    /// </summary>
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

    /// <summary>
    /// Converts a card loaded from PostgreSQL into the same API response model.
    ///
    /// ReadingCardDetails already contains flattened persisted values, so its
    /// properties are accessed directly.
    /// </summary>
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