using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Tarot.Infrastructure.Persistence;
using Xunit;

namespace Tarot.IntegrationTests.Infrastructure;

/// <summary>
/// Manages a temporary PostgreSQL container used by the integration tests.
///
/// xUnit creates this fixture once for the test class that declares
/// IClassFixture<PostgreSqlFixture>.
///
/// The container is completely separate from the PostgreSQL container used
/// by the application during development.
/// </summary>
public sealed class PostgreSqlFixture : IAsyncLifetime
{
    /*
     * The PostgreSqlBuilder describes the temporary database container.
     *
     * We pin PostgreSQL to version 16 because the application also uses
     * PostgreSQL 16 in docker-compose.yml.
     *
     * Testcontainers automatically assigns an available host port, so the
     * test database does not need to use the development database's port 5432.
     */
    private readonly PostgreSqlContainer _container =
        new PostgreSqlBuilder("postgres:16")
            .WithDatabase("arcana_integration_tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

    /// <summary>
    /// Returns the dynamically generated connection string for the temporary
    /// PostgreSQL container.
    ///
    /// It contains the container's assigned host port, database, username,
    /// and password.
    /// </summary>
    public string ConnectionString => _container.GetConnectionString();

    /// <summary>
    /// Called automatically by xUnit before tests that use this fixture run.
    ///
    /// Docker must be available because this method starts a real
    /// PostgreSQL container.
    /// </summary>
    public Task InitializeAsync()
    {
        return _container.StartAsync();
    }

    /// <summary>
    /// Called automatically by xUnit after all tests using this fixture finish.
    ///
    /// Disposing the Testcontainer removes the temporary PostgreSQL container.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    /// <summary>
    /// Creates a new EF Core DbContext connected to the temporary PostgreSQL
    /// database.
    ///
    /// Each test should create and dispose its own DbContext. The PostgreSQL
    /// container is shared, but DbContext instances are not shared.
    /// </summary>
    public TarotDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TarotDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new TarotDbContext(options);
    }

    /// <summary>
    /// Deletes the current test database and recreates it by applying the
    /// application's real EF Core migrations.
    ///
    /// The test class calls this before each test, ensuring that tests do not
    /// depend on data inserted by earlier tests.
    /// </summary>
    public async Task RecreateDatabaseAsync()
    {
        await using var dbContext = CreateDbContext();

        // Remove the existing test database, including all tables and data.
        await dbContext.Database.EnsureDeletedAsync();

        // Recreate the database schema by applying the real migrations from
        // Tarot.Infrastructure/Persistence/Migrations.
        await dbContext.Database.MigrateAsync();
    }

    /// <summary>
    /// Deletes the database without recreating it.
    ///
    /// This method is used by the migration test to prove that migrations can
    /// initialize a genuinely empty PostgreSQL database.
    /// </summary>
    public async Task DeleteDatabaseAsync()
    {
        await using var dbContext = CreateDbContext();

        await dbContext.Database.EnsureDeletedAsync();
    }
}