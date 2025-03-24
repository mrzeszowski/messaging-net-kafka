using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Chat.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventLog",
                schema: "chat",
                columns: table => new
                {
                    LocalOffset = table.Column<long>(type: "bigint", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Payload = table.Column<byte[]>(type: "bytea", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    Topic = table.Column<string>(type: "text", nullable: false),
                    PartitionKey = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventLog", x => x.LocalOffset);
                });

            migrationBuilder.CreateTable(
                name: "EventOutbox",
                schema: "chat",
                columns: table => new
                {
                    LocalOffset = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Payload = table.Column<byte[]>(type: "bytea", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    Topic = table.Column<string>(type: "text", nullable: false),
                    PartitionKey = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventOutbox", x => x.LocalOffset);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_Topic",
                schema: "chat",
                table: "EventLog",
                column: "Topic");

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_Type",
                schema: "chat",
                table: "EventLog",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_Type_PartitionKey",
                schema: "chat",
                table: "EventLog",
                columns: new[] { "Type", "PartitionKey" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventLog",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "EventOutbox",
                schema: "chat");
        }
    }
}
