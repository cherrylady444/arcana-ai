using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tarot.Infrastructure.Persistence.Migrations;

public partial class AddCustomSpreadReadingSnapshot : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "is_custom_spread",
            table: "readings",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "spread_description",
            table: "readings",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "is_custom_spread",
            table: "readings");

        migrationBuilder.DropColumn(
            name: "spread_description",
            table: "readings");
    }
}
