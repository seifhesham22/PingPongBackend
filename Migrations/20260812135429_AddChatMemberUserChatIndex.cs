using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PingPong.API.Migrations
{
    /// <inheritdoc />
    public partial class AddChatMemberUserChatIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChatMembers_UserId",
                table: "ChatMembers");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMembers_UserId_ChatId",
                table: "ChatMembers",
                columns: new[] { "UserId", "ChatId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChatMembers_UserId_ChatId",
                table: "ChatMembers");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMembers_UserId",
                table: "ChatMembers",
                column: "UserId");
        }
    }
}
