using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tarot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameIdColumnsToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "readings",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "reading_cards",
                newName: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "id",
                table: "readings",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "reading_cards",
                newName: "Id");
        }
    }
}
