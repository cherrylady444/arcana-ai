using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tarot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialReadingPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "readings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_context = table.Column<string>(type: "text", nullable: true),
                    spread_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    spread_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_readings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "reading_cards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    reading_id = table.Column<Guid>(type: "uuid", nullable: false),
                    position_order = table.Column<int>(type: "integer", nullable: false),
                    position_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    position_title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    position_prompt = table.Column<string>(type: "text", nullable: false),
                    position_description = table.Column<string>(type: "text", nullable: false),
                    card_id = table.Column<Guid>(type: "uuid", nullable: false),
                    card_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    arcana = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    suit = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    orientation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    meaning = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reading_cards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reading_cards_readings_reading_id",
                        column: x => x.reading_id,
                        principalTable: "readings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_reading_cards_reading_id_position_order",
                table: "reading_cards",
                columns: new[] { "reading_id", "position_order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reading_cards");

            migrationBuilder.DropTable(
                name: "readings");
        }
    }
}
