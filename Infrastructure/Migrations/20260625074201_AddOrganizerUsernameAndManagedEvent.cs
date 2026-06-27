using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizerUsernameAndManagedEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ManagedEventId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_users_username",
                table: "Users",
                column: "Username",
                unique: true,
                filter: "\"Username\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ManagedEventId",
                table: "Users",
                column: "ManagedEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Events_ManagedEventId",
                table: "Users",
                column: "ManagedEventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Events_ManagedEventId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "idx_users_username",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ManagedEventId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ManagedEventId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Users");
        }
    }
}
