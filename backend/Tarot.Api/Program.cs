using Serilog;
using Tarot.Application.Readings;

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

// IMPORTANT
// tells ASP.NET:
// Whenever an endpoint asks for IReadingService,
// give it a FakeReadingService.
// builder.Services.AddScoped<IReadingService, FakeReadingService>();

builder.Services.AddScoped<IReadingService, FakeReadingService>();

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

    return Results.Ok(result);
});

app.Run();

public sealed record CreateReadingRequest(
    string Question
);