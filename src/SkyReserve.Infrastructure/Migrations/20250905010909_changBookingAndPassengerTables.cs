using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyReserve.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class changBookingAndPassengerTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Passengers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BookingRef",
                table: "Bookings",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Passengers_UserId",
                table: "Passengers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingRef",
                table: "Bookings",
                column: "BookingRef",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Passengers_AspNetUsers_UserId",
                table: "Passengers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Passengers_AspNetUsers_UserId",
                table: "Passengers");

            migrationBuilder.DropIndex(
                name: "IX_Passengers_UserId",
                table: "Passengers");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_BookingRef",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Passengers");

            migrationBuilder.DropColumn(
                name: "BookingRef",
                table: "Bookings");
        }
    }
}
