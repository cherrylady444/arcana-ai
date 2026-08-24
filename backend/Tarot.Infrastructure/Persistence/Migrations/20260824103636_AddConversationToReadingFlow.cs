using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tarot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConversationToReadingFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "source_conversation_id",
                table: "readings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "suggested_action",
                table: "conversation_messages",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_readings_source_conversation_id",
                table: "readings",
                column: "source_conversation_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_readings_conversations_source_conversation_id",
                table: "readings",
                column: "source_conversation_id",
                principalTable: "conversations",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_readings_conversations_source_conversation_id",
                table: "readings");

            migrationBuilder.DropIndex(
                name: "IX_readings_source_conversation_id",
                table: "readings");

            migrationBuilder.DropColumn(
                name: "source_conversation_id",
                table: "readings");

            migrationBuilder.DropColumn(
                name: "suggested_action",
                table: "conversation_messages");
        }
    }
}
