using Serilog;
using Tarot.Application.Cards;
using Tarot.Application.Readings;
using Tarot.Domain;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddScoped<ICardDrawingService, CardDrawingService>();
builder.Services.AddScoped<IReadingService, CreateReadingService>();

var app = builder.Build();

app.UseCors("Frontend");

app.MapGet("/", () => Results.Ok(new
{
    service = "tarot-backend",
    status = "ok"
}));

app.MapHealthChecks("/health");

app.MapPost("/api/readings", (
    CreateReadingRequest request,
    IReadingService readingService) =>
{
    if (string.IsNullOrWhiteSpace(request.Question))
    {
        return Results.BadRequest(new
        {
            error = "Question is required."
        });
    }

    var result = readingService.CreateReading(
        new CreateReadingCommand(request.Question)
    );

    var response = new ReadingResponse(
        Id: result.Id,
        Question: result.Question,
        Cards: result.Cards.Select(ToResponse).ToList(),
        CreatedAtUtc: result.CreatedAtUtc
    );

    return Results.Ok(response);
});

app.Run();

static DrawnCardResponse ToResponse(DrawnCard drawnCard)
{
    return new DrawnCardResponse(
        Id: drawnCard.Card.Id,
        Name: drawnCard.Card.Name,
        Arcana: drawnCard.Card.Arcana,
        Meaning: drawnCard.Card.Meaning,
        Orientation: drawnCard.Orientation.ToString()
    );
}

public sealed record CreateReadingRequest(
    string Question
);

public sealed record ReadingResponse(
    Guid Id,
    string Question,
    IReadOnlyCollection<DrawnCardResponse> Cards,
    DateTimeOffset CreatedAtUtc
);

public sealed record DrawnCardResponse(
    Guid Id,
    string Name,
    string Arcana,
    string Meaning,
    string Orientation
);
