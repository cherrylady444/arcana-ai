using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tarot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReadingInterpretationPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reading_interpretations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reading_id = table.Column<Guid>(type: "uuid", nullable: false),
                    overall_theme = table.Column<string>(type: "text", nullable: false),
                    synthesis = table.Column<string>(type: "text", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reading_interpretations", x => x.id);
                    table.ForeignKey(
                        name: "FK_reading_interpretations_readings_reading_id",
                        column: x => x.reading_id,
                        principalTable: "readings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reading_interpretation_reflection_questions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reading_interpretation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    question_order = table.Column<int>(type: "integer", nullable: false),
                    question = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reading_interpretation_reflection_questions", x => x.id);
                    table.ForeignKey(
                        name: "FK_reading_interpretation_reflection_questions_reading_interpr~",
                        column: x => x.reading_interpretation_id,
                        principalTable: "reading_interpretations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reading_position_interpretations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reading_interpretation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    position_order = table.Column<int>(type: "integer", nullable: false),
                    interpretation = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reading_position_interpretations", x => x.id);
                    table.ForeignKey(
                        name: "FK_reading_position_interpretations_reading_interpretations_re~",
                        column: x => x.reading_interpretation_id,
                        principalTable: "reading_interpretations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_reading_interpretation_reflection_questions_reading_interpr~",
                table: "reading_interpretation_reflection_questions",
                columns: new[] { "reading_interpretation_id", "question_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reading_interpretations_reading_id_created_at_utc",
                table: "reading_interpretations",
                columns: new[] { "reading_id", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_reading_position_interpretations_reading_interpretation_id_~",
                table: "reading_position_interpretations",
                columns: new[] { "reading_interpretation_id", "position_order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reading_interpretation_reflection_questions");

            migrationBuilder.DropTable(
                name: "reading_position_interpretations");

            migrationBuilder.DropTable(
                name: "reading_interpretations");
        }
    }
}
