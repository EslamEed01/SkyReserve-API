using SkyReserve.Application.Passenger.DTOS;

namespace SkyReserve.Application.Booking.DTOS
{
    public class CreateBookingWithPassengersRequest
    {
        public int FlightId { get; set; }
        public decimal TotalAmount { get; set; }
        public List<CreatePassengerDto> Passengers { get; set; } = new();
    }
}
