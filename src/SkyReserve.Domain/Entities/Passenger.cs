namespace SkyReserve.Domain.Entities
{
    public class Passenger
    {
        public int PassengerId { get; set; }
        public int BookingId { get; set; }
        public string? UserId { get; set; } 
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PassportNumber { get; set; }
        public string Nationality { get; set; }
        public bool IsGuestPassenger => string.IsNullOrEmpty(UserId);

        // Navigation properties
        public Booking Booking { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
