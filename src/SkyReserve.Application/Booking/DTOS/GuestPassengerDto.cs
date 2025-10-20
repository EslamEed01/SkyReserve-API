namespace SkyReserve.Application.Booking.DTOS
{
    public class GuestPassengerDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string PassportNumber { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
    }
}
