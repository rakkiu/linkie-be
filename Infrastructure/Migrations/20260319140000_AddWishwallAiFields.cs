using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWishwallAiFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiLabel",
                table: "WishwallMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiReason",
                table: "WishwallMessages",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiLabel",
                table: "WishwallMessages");

            migrationBuilder.DropColumn(
                name: "AiReason",
                table: "WishwallMessages");
        }
    }
}
