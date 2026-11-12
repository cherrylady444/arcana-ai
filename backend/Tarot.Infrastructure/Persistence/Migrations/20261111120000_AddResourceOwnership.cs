using Microsoft.EntityFrameworkCore.Migrations;

namespace Tarot.Infrastructure.Persistence.Migrations;

public partial class AddResourceOwnership : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var table in new[] { "readings", "conversations" })
        {
            migrationBuilder.AddColumn<string>("owner_issuer", table, type: "text", nullable: true);
            migrationBuilder.AddColumn<string>("owner_subject", table, type: "text", nullable: true);
            var time = table == "readings" ? "created_at_utc" : "updated_at_utc";
            migrationBuilder.CreateIndex($"ix_{table}_owner_time", table,
                new[] { "owner_issuer", "owner_subject", time, "id" });
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        foreach (var table in new[] { "readings", "conversations" })
        {
            migrationBuilder.DropIndex($"ix_{table}_owner_time", table);
            migrationBuilder.DropColumn("owner_subject", table);
            migrationBuilder.DropColumn("owner_issuer", table);
        }
    }
}
