using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Tarot.Infrastructure.Persistence;

#nullable disable

namespace Tarot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(TarotDbContext))]
[Migration("20260817224500_AddConversationPersistence")]
public partial class AddConversationPersistence : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "conversations",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                reading_id = table.Column<Guid>(type: "uuid", nullable: true),
                interpretation_id = table.Column<Guid>(type: "uuid", nullable: true),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_conversations", x => x.id);
                table.ForeignKey(
                    name: "FK_conversations_reading_interpretations_interpretation_id",
                    column: x => x.interpretation_id,
                    principalTable: "reading_interpretations",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_conversations_readings_reading_id",
                    column: x => x.reading_id,
                    principalTable: "readings",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "conversation_messages",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                conversation_id = table.Column<Guid>(type: "uuid", nullable: false),
                sequence = table.Column<int>(type: "integer", nullable: false),
                role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                content = table.Column<string>(type: "text", nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_conversation_messages", x => x.id);
                table.ForeignKey(
                    name: "FK_conversation_messages_conversations_conversation_id",
                    column: x => x.conversation_id,
                    principalTable: "conversations",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });


        migrationBuilder.AddColumn<Guid>(
            name: "source_conversation_id",
            table: "reading_interpretations",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_reading_interpretations_source_conversation_id",
            table: "reading_interpretations",
            column: "source_conversation_id");

        migrationBuilder.AddForeignKey(
            name: "FK_reading_interpretations_conversations_source_conversation_id",
            table: "reading_interpretations",
            column: "source_conversation_id",
            principalTable: "conversations",
            principalColumn: "id",
            onDelete: ReferentialAction.SetNull);

        migrationBuilder.CreateIndex(
            name: "IX_conversation_messages_conversation_id_sequence",
            table: "conversation_messages",
            columns: new[] { "conversation_id", "sequence" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_conversations_interpretation_id",
            table: "conversations",
            column: "interpretation_id");

        migrationBuilder.CreateIndex(
            name: "IX_conversations_reading_id",
            table: "conversations",
            column: "reading_id");

        migrationBuilder.CreateIndex(
            name: "IX_conversations_updated_at_utc",
            table: "conversations",
            column: "updated_at_utc");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_reading_interpretations_conversations_source_conversation_id",
            table: "reading_interpretations");

        migrationBuilder.DropIndex(
            name: "IX_reading_interpretations_source_conversation_id",
            table: "reading_interpretations");

        migrationBuilder.DropColumn(
            name: "source_conversation_id",
            table: "reading_interpretations");

        migrationBuilder.DropTable(name: "conversation_messages");
        migrationBuilder.DropTable(name: "conversations");
    }
}
