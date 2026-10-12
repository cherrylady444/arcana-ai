using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Tarot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(TarotDbContext))]
[Migration("20260914120000_AddAuthenticationSessions")]
public sealed class AddAuthenticationSessions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        CREATE TABLE authentication_sessions (
            id text PRIMARY KEY,
            protected_ticket bytea NOT NULL,
            expires_at_utc timestamptz NOT NULL,
            absolute_expires_at_utc timestamptz NOT NULL,
            validated_at_utc timestamptz NOT NULL
        );
        CREATE INDEX ix_authentication_sessions_expiry ON authentication_sessions (expires_at_utc);
        """);

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql("DROP TABLE authentication_sessions;");
}
