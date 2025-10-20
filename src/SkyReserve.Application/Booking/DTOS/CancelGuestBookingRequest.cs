namespace SkyReserve.Application.Booking.DTOS
{
    public class CancelGuestBookingRequest
    {
        public string BookingRef { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string CancellationReason { get; set; } = string.Empty;
    }
}
