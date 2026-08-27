using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PingPong.API.Migrations
{
    /// <inheritdoc />
    public partial class everyOne_Role_system : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServerRoles_ServerId",
                table: "ServerRoles");

            migrationBuilder.AddColumn<bool>(
                name: "IsEveryone",
                table: "ServerRoles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ServerRoles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ServerRoles_ServerId",
                table: "ServerRoles",
                column: "ServerId",
                unique: true,
                filter: "\"IsEveryone\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServerRoles_ServerId",
                table: "ServerRoles");

            migrationBuilder.DropColumn(
                name: "IsEveryone",
                table: "ServerRoles");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ServerRoles");

            migrationBuilder.CreateIndex(
                name: "IX_ServerRoles_ServerId",
                table: "ServerRoles",
                column: "ServerId",
                unique: true);
        }
    }
}
