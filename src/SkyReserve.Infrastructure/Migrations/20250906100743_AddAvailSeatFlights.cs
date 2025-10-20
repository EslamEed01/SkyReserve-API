using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyReserve.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAvailSeatFlights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AvailableSeats",
                table: "Flights",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalSeats",
                table: "Flights",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableSeats",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "TotalSeats",
                table: "Flights");
        }
    }
}
