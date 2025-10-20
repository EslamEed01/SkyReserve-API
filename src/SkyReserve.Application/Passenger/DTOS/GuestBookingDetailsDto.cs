namespace SkyReserve.Application.Passenger.DTOS
{
    public class GuestBookingDetailsDto
    {
        public string BookingRef { get; set; } = string.Empty;
        public string BookingStatus { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public decimal TotalAmount { get; set; }

        public string FlightNumber { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string DepartureAirport { get; set; } = string.Empty;
        public string ArrivalAirport { get; set; } = string.Empty;
        public string FlightStatus { get; set; } = string.Empty;

        public List<PassengerDto> Passengers { get; set; } = new List<PassengerDto>();
        public string PaymentStatus { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime? PaymentDate { get; set; }
    }
}