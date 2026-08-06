using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PingPong.API.Migrations
{
    /// <inheritdoc />
    public partial class Add_IsUnique_To_Block_Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Blocks_BlockerId_BlockedId",
                table: "Blocks");

            migrationBuilder.DropColumn(
                name: "BlockedByUserId",
                table: "Friendships");

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_BlockerId_BlockedId",
                table: "Blocks",
                columns: new[] { "BlockerId", "BlockedId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Blocks_BlockerId_BlockedId",
                table: "Blocks");

            migrationBuilder.AddColumn<Guid>(
                name: "BlockedByUserId",
                table: "Friendships",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_BlockerId_BlockedId",
                table: "Blocks",
                columns: new[] { "BlockerId", "BlockedId" });
        }
    }
}
