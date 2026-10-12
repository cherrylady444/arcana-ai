using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Tarot.Api.Security;

/// <summary>
/// Opaque browser references point to encrypted authentication tickets here.
/// This technical table is managed by an EF SQL migration, not a domain entity.
/// A database row lock serializes refresh with other refreshes and logout.
/// </summary>
public sealed class PostgresTicketStore : ITicketStore
{
    private readonly NpgsqlDataSource _database;
    private readonly IDataProtector _protector;
    private readonly ArcanaSecurityOptions _options;
    private readonly IHttpClientFactory _clients;
    private readonly ILogger<PostgresTicketStore> _logger;

    public PostgresTicketStore(NpgsqlDataSource database, IDataProtectionProvider protection,
        IOptions<ArcanaSecurityOptions> options, IHttpClientFactory clients, ILogger<PostgresTicketStore> logger)
    {
        _database = database;
        _protector = protection.CreateProtector("Arcana.AuthenticationTicket.v1");
        _options = options.Value;
        _clients = clients;
        _logger = logger;
    }

    private static string Hash(string key) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)));
    private byte[] Protect(AuthenticationTicket ticket) => _protector.Protect(TicketSerializer.Default.Serialize(ticket));

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var now = DateTimeOffset.UtcNow;
        var absolute = now.AddHours(_options.AbsoluteHours);
        ticket.Properties.ExpiresUtc = absolute;
        await using var command = _database.CreateCommand("""
            INSERT INTO authentication_sessions
                (id, protected_ticket, expires_at_utc, absolute_expires_at_utc, validated_at_utc)
            VALUES ($1, $2, $3, $4, $5)
            """);
        command.Parameters.AddWithValue(Hash(key));
        command.Parameters.AddWithValue(Protect(ticket));
        command.Parameters.AddWithValue(now.AddMinutes(_options.IdleMinutes));
        command.Parameters.AddWithValue(absolute);
        command.Parameters.AddWithValue(now);
        await command.ExecuteNonQueryAsync();
        return key;
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        // CookieAuthentication calls this before accepting any session. No token
        // or stale principal is trusted from the browser itself.
        await using var connection = await _database.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await using var command = new NpgsqlCommand("""
            SELECT protected_ticket, expires_at_utc, absolute_expires_at_utc, validated_at_utc
            FROM authentication_sessions WHERE id = $1 FOR UPDATE
            """, connection, transaction);
        command.Parameters.AddWithValue(Hash(key));
        byte[] bytes;
        DateTime expires, absolute, validated;
        await using (var reader = await command.ExecuteReaderAsync())
        {
            if (!await reader.ReadAsync()) return null;
            bytes = reader.GetFieldValue<byte[]>(0);
            expires = reader.GetDateTime(1);
            absolute = reader.GetDateTime(2);
            validated = reader.GetDateTime(3);
        }

        var now = DateTimeOffset.UtcNow;
        AuthenticationTicket? ticket = null;
        if (expires > now.UtcDateTime && absolute > now.UtcDateTime)
        {
            try { ticket = TicketSerializer.Default.Deserialize(_protector.Unprotect(bytes)); }
            catch (CryptographicException) { /* Unreadable sessions fail closed. */ }
        }

        if (ticket is not null && validated.AddSeconds(_options.RevalidationSeconds) <= now.UtcDateTime)
        {
            var refresher = new KeycloakTokenRefresher(_clients.CreateClient("KeycloakRefresh"), Options.Create(_options));
            try
            {
                if (!await refresher.RefreshAsync(ticket, CancellationToken.None)) ticket = null;
                else validated = DateTime.UtcNow;
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException
                or System.Text.Json.JsonException or KeyNotFoundException or InvalidOperationException)
            {
                // Never log the response body or exception: these may contain credentials.
                _logger.LogWarning("Keycloak session revalidation failed; this request is unauthenticated.");
                return null;
            }
        }

        if (ticket is null)
        {
            await using var delete = new NpgsqlCommand("DELETE FROM authentication_sessions WHERE id = $1", connection, transaction);
            delete.Parameters.AddWithValue(Hash(key));
            await delete.ExecuteNonQueryAsync();
        }
        else
        {
            await using var update = new NpgsqlCommand("""
                UPDATE authentication_sessions SET protected_ticket = $2, expires_at_utc = $3,
                    validated_at_utc = $4 WHERE id = $1
                """, connection, transaction);
            update.Parameters.AddWithValue(Hash(key));
            update.Parameters.AddWithValue(Protect(ticket));
            update.Parameters.AddWithValue(new DateTimeOffset(new[]
                { absolute, now.AddMinutes(_options.IdleMinutes).UtcDateTime }.Min()));
            update.Parameters.AddWithValue(validated);
            await update.ExecuteNonQueryAsync();
        }
        await transaction.CommitAsync();
        return ticket;
    }

    // Sliding cookie renewal is disabled. Never write an older copy of tokens
    // over a freshly rotated refresh token, or recreate a deleted session.
    public Task RenewAsync(string key, AuthenticationTicket ticket) => Task.CompletedTask;

    public async Task RemoveAsync(string key)
    {
        await using var command = _database.CreateCommand("DELETE FROM authentication_sessions WHERE id = $1");
        command.Parameters.AddWithValue(Hash(key));
        await command.ExecuteNonQueryAsync();
    }
}
