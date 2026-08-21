using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PingPong.API.Migrations
{
    /// <inheritdoc />
    public partial class addserverinvitationnavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ServerId1",
                table: "ServerInvitations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ServerInvitations_ServerId1",
                table: "ServerInvitations",
                column: "ServerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ServerInvitations_Servers_ServerId1",
                table: "ServerInvitations",
                column: "ServerId1",
                principalTable: "Servers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServerInvitations_Servers_ServerId1",
                table: "ServerInvitations");

            migrationBuilder.DropIndex(
                name: "IX_ServerInvitations_ServerId1",
                table: "ServerInvitations");

            migrationBuilder.DropColumn(
                name: "ServerId1",
                table: "ServerInvitations");
        }
    }
}
