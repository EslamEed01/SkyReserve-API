using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyReserve.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StripePaymentIntentId",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Payments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "applicationUserId",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_applicationUserId",
                table: "Payments",
                column: "applicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_AspNetUsers_applicationUserId",
                table: "Payments",
                column: "applicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_AspNetUsers_applicationUserId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_applicationUserId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "StripePaymentIntentId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "applicationUserId",
                table: "Payments");
        }
    }
}
