using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PingPong.API.Migrations
{
    /// <inheritdoc />
    public partial class AddChatLastMessageAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastMessageAt",
                table: "Chats",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Chats_LastMessageAt_Id",
                table: "Chats",
                columns: new[] { "LastMessageAt", "Id" },
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chats_LastMessageAt_Id",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "LastMessageAt",
                table: "Chats");
        }
    }
}
