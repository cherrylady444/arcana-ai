using Serilog;

using Tarot.Api.Endpoints;
using Tarot.Application.Cards;
using Tarot.Application.Readings;
using Tarot.Application.Spreads;

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

// PostgreSQL connection. In Docker Compose, the environment variable
// ConnectionStrings__DefaultConnection overrides local appsettings values.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is not configured.");

builder.Services.AddDbContext<TarotDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    });
});

builder.Services.AddScoped<ICardDrawingService, CardDrawingService>();
builder.Services.AddScoped<IReadingService, CreateReadingService>();
builder.Services.AddScoped<IGetReadingByIdService, GetReadingByIdService>();
builder.Services.AddScoped<IReadingRepository, PostgresReadingRepository>();
builder.Services.AddScoped<IListReadingsService, ListReadingsService>();
builder.Services.AddSingleton<ISpreadCatalog, BuiltInSpreadCatalog>();

var app = builder.Build();

// This is enabled by Docker Compose for the current single-instance
// development environment. For a mature production deployment, migrations
// should normally be applied as a separate deployment step.
if (builder.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    app.Logger.LogInformation("Applying pending database migrations...");

    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<TarotDbContext>();
    await dbContext.Database.MigrateAsync();

    app.Logger.LogInformation("Database migrations applied successfully.");
}

app.UseCors("Frontend");

app.MapGet("/", () => Results.Ok(new
{
    service = "tarot-backend",
    status = "ok"
}));

app.MapHealthChecks("/health");

app.MapSpreadEndpoints();
app.MapReadingEndpoints();


// app.MapGet("/api/spreads", GetSpreadSummaries);

// app.MapGet("/api/spreads/{spreadKey}", GetSpreadDetails);
// // POST endpoint for creating a new reading
// app.MapPost("/api/readings", CreateReading);
// // GET endpoint for retrieving a paginated list of readings
// app.MapGet("/api/readings", ListReadings);

// app.MapGet("/api/readings/{id:guid}", GetReadingById);

app.Run();

/*
 * Exposing Program as a public partial class will make it easier to add
 * HTTP-level integration tests later with WebApplicationFactory<Program>.
 */
public partial class Program;