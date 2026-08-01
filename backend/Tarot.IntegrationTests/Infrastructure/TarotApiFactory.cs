using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tarot.Infrastructure.Persistence;
using Tarot.Application.Interpretations;

namespace Tarot.IntegrationTests.Infrastructure;

/// <summary>
/// Starts the real Tarot.Api application in ASP.NET Core's in-memory test
/// server while replacing its normal TarotDbContext registration with the
/// temporary PostgreSQL database supplied by Testcontainers.
/// = test version of Tarot.Api
/// </summary>
public sealed class TarotApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public TarotApiFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public FakeReadingInterpreter ReadingInterpreter { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        /*
         * Program.cs reads DefaultConnection while building the application.
         * Supplying the Testcontainers connection string here ensures even the
         * initial registration can never point at the normal development DB.
         */
        builder.UseSetting(
            "ConnectionStrings:DefaultConnection",
            _connectionString);

        /*
         * The fixture applies migrations explicitly when it recreates the test
         * database. Keeping startup migrations disabled avoids two competing
         * owners of database initialization in the test suite.
         */
        builder.UseSetting(
            "Database:ApplyMigrationsOnStartup",
            "false");

        builder.ConfigureServices(services =>
        {
            /*
             * AddDbContext registers the DbContext itself, DbContextOptions,
             * and EF Core option-configuration services. Remove all three so
             * the production registration cannot continue contributing its
             * Npgsql connection configuration in the test host.
             */
            services.RemoveAll<TarotDbContext>();
            services.RemoveAll<DbContextOptions<TarotDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<TarotDbContext>>();

            services.AddDbContext<TarotDbContext>(options =>
            {
                options.UseNpgsql(_connectionString);
            });
            // This means:
            // production API → PythonReadingInterpreterClient
            // API integration tests → FakeReadingInterpreter
            services.RemoveAll<IReadingInterpreter>();

            services.AddSingleton<IReadingInterpreter>(
                ReadingInterpreter);
        });
    }
}
