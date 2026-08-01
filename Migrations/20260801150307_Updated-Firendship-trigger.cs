using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PingPong.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedFirendshiptrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Friendships_AspNetUsers_AddresseeId",
                table: "Friendships");

            migrationBuilder.DropForeignKey(
                name: "FK_Friendships_AspNetUsers_RequesterId",
                table: "Friendships");

            migrationBuilder.DropIndex(
                name: "IX_Friendships_RequesterId_AddresseeId",
                table: "Friendships");

            migrationBuilder.RenameColumn(
                name: "AddresseeId",
                table: "Friendships",
                newName: "SecondUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Friendships_AddresseeId",
                table: "Friendships",
                newName: "IX_Friendships_SecondUserId");

            migrationBuilder.AddColumn<Guid>(
                name: "FirstUserId",
                table: "Friendships",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_FirstUserId_SecondUserId",
                table: "Friendships",
                columns: new[] { "FirstUserId", "SecondUserId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Friendships_AspNetUsers_FirstUserId",
                table: "Friendships",
                column: "FirstUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Friendships_AspNetUsers_SecondUserId",
                table: "Friendships",
                column: "SecondUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Friendships_AspNetUsers_FirstUserId",
                table: "Friendships");

            migrationBuilder.DropForeignKey(
                name: "FK_Friendships_AspNetUsers_SecondUserId",
                table: "Friendships");

            migrationBuilder.DropIndex(
                name: "IX_Friendships_FirstUserId_SecondUserId",
                table: "Friendships");

            migrationBuilder.DropColumn(
                name: "FirstUserId",
                table: "Friendships");

            migrationBuilder.RenameColumn(
                name: "SecondUserId",
                table: "Friendships",
                newName: "AddresseeId");

            migrationBuilder.RenameIndex(
                name: "IX_Friendships_SecondUserId",
                table: "Friendships",
                newName: "IX_Friendships_AddresseeId");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_RequesterId_AddresseeId",
                table: "Friendships",
                columns: new[] { "RequesterId", "AddresseeId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Friendships_AspNetUsers_AddresseeId",
                table: "Friendships",
                column: "AddresseeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Friendships_AspNetUsers_RequesterId",
                table: "Friendships",
                column: "RequesterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
