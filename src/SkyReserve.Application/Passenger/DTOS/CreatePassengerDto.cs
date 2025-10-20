namespace SkyReserve.Application.Passenger.DTOS
{
    public class CreatePassengerDto
    {
        public int BookingId { get; set; }
        public string? UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string PassportNumber { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
    }
}