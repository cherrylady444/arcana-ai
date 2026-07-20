using Serilog;
using Tarot.Application.Cards;
using Tarot.Application.Readings;
using Tarot.Application.Spreads;
using Tarot.Domain;

using Microsoft.EntityFrameworkCore;
using Tarot.Infrastructure.Persistence;
using Tarot.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5000");

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console();
});

builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",
                "http://127.0.0.1:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// For the database connection 
builder.Services.AddDbContext<TarotDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<ICardDrawingService, CardDrawingService>();
builder.Services.AddScoped<IReadingService, CreateReadingService>();
builder.Services.AddScoped<IGetReadingByIdService, GetReadingByIdService>();
builder.Services.AddScoped<IReadingRepository, PostgresReadingRepository>();
builder.Services.AddScoped<IListReadingsService, ListReadingsService>();
builder.Services.AddSingleton<ISpreadCatalog, BuiltInSpreadCatalog>();

var app = builder.Build();

app.UseCors("Frontend");

app.MapGet("/", () => Results.Ok(new
{
    service = "tarot-backend",
    status = "ok"
}));

app.MapHealthChecks("/health");

app.MapGet("/api/spreads", GetSpreadSummaries);

app.MapGet("/api/spreads/{spreadKey}", GetSpreadDetails);
// POST endpoint for creating a new reading
app.MapPost("/api/readings", CreateReading);
// GET endpoint for retrieving a paginated list of readings
app.MapGet("/api/readings", ListReadings);

app.MapGet("/api/readings/{id:guid}", GetReadingById);

app.Run();

static IResult GetSpreadSummaries(ISpreadCatalog spreadCatalog)
{
    var response = spreadCatalog.GetBuiltInSpreads()
        .Select(ToSummaryResponse)
        .ToList();

    return Results.Ok(response);
}

static IResult GetSpreadDetails(string spreadKey, ISpreadCatalog spreadCatalog)
{
    var spread = spreadCatalog.GetByKey(spreadKey);

    if (spread is null)
    {
        return Results.NotFound(new
        {
            error = $"Spread '{spreadKey}' was not found."
        });
    }

    return Results.Ok(ToDetailResponse(spread));
}

static async Task<IResult> CreateReading(
    CreateReadingRequest request,
    IReadingService readingService,
    CancellationToken cancellationToken)
{
    try
    {
        var result = await readingService.CreateReadingAsync(
            new CreateReadingCommand(
                SpreadKey: request.SpreadKey,
                UserContext: request.UserContext
            ),
            cancellationToken
        );

        var response = new ReadingResponse(
            Id: result.Id,
            UserContext: result.UserContext,
            SpreadKey: result.SpreadKey,
            SpreadName: result.SpreadName,
            Cards: result.Cards
                .Select(ToCreatedReadingCardResponse)
                .ToList(),
            CreatedAtUtc: result.CreatedAtUtc
        );

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

static async Task<IResult> GetReadingById(
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
        CreatedAtUtc: reading.CreatedAtUtc
    );

    return Results.Ok(response);
}

static async Task<IResult> ListReadings(
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

static SpreadSummaryResponse ToSummaryResponse(SpreadDefinition spread)
{
    return new SpreadSummaryResponse(
        Key: spread.Key,
        Name: spread.Name,
        ShortDescription: spread.ShortDescription,
        Intent: spread.Intent,
        CardCount: spread.CardCount
    );
}

static SpreadDetailResponse ToDetailResponse(SpreadDefinition spread)
{
    return new SpreadDetailResponse(
        Key: spread.Key,
        Name: spread.Name,
        ShortDescription: spread.ShortDescription,
        Description: spread.Description,
        Intent: spread.Intent,
        CardCount: spread.CardCount,
        Positions: spread.Positions
            .OrderBy(position => position.Order)
            .Select(ToSpreadPositionResponse)
            .ToList()
    );
}

static SpreadPositionResponse ToSpreadPositionResponse(SpreadPositionDefinition position)
{
    return new SpreadPositionResponse(
        Order: position.Order,
        Key: position.Key,
        Title: position.Title,
        Prompt: position.Prompt,
        Description: position.Description
    );
}

static ReadingCardResponse ToCreatedReadingCardResponse(
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
        Meaning: readingCard.Meaning
    );
}

// C# supports overloaded methods, so both can have the same name because their parameter types differ:

// ToReadingCardResponse(ReadingCard readingCard)
// ToReadingCardResponse(ReadingCardDetails readingCard)

//but not when they are both in this top-level Program.cs.

static ReadingCardResponse ToSavedReadingCardResponse(
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
        Meaning: readingCard.Meaning
    );
}

public sealed record CreateReadingRequest(
    string? SpreadKey,
    string? UserContext
);

public sealed record ReadingResponse(
    Guid Id,
    string? UserContext,
    string SpreadKey,
    string SpreadName,
    IReadOnlyCollection<ReadingCardResponse> Cards,
    DateTimeOffset CreatedAtUtc
);

public sealed record ReadingCardResponse(
    int PositionOrder,
    string PositionKey,
    string PositionTitle,
    string PositionPrompt,
    string PositionDescription,
    Guid CardId,
    string CardName,
    string Arcana,
    string? Suit,
    string Orientation,
    string Meaning
);

public sealed record SpreadSummaryResponse(
    string Key,
    string Name,
    string ShortDescription,
    string Intent,
    int CardCount
);

public sealed record SpreadDetailResponse(
    string Key,
    string Name,
    string ShortDescription,
    string Description,
    string Intent,
    int CardCount,
    IReadOnlyCollection<SpreadPositionResponse> Positions
);

public sealed record SpreadPositionResponse(
    int Order,
    string Key,
    string Title,
    string Prompt,
    string Description
);
