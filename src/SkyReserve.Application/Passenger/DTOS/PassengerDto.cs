namespace SkyReserve.Application.Passenger.DTOS
{
    public class PassengerDto
    {
        public int PassengerId { get; set; }
        public int BookingId { get; set; }
        public string? UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string PassportNumber { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public bool IsGuestPassenger => string.IsNullOrEmpty(UserId);
        public string? BookingStatus { get; set; }
        public DateTime? BookingDate { get; set; }
    }
}