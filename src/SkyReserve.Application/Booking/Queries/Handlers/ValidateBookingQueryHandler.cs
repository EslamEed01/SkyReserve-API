using MediatR;
using SkyReserve.Application.Booking.Queries.Models;
using SkyReserve.Application.Interfaces;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Booking.Queries.Handlers
{
    public class ValidateBookingQueryHandler : IRequestHandler<ValidateBookingQuery, bool>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly IPassengerRepository _passengerRepository;

        public ValidateBookingQueryHandler(
            IBookingRepository bookingRepository,
            IFlightRepository flightRepository,
            IPassengerRepository passengerRepository)
        {
            _bookingRepository = bookingRepository;
            _flightRepository = flightRepository;
            _passengerRepository = passengerRepository;
        }

        public async Task<bool> Handle(ValidateBookingQuery request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
                return false;

            var flight = await _flightRepository.GetByIdAsync(booking.FlightId);
            if (flight == null)
                return false;

            var passengers = await _passengerRepository.GetByBookingIdAsync(request.BookingId);
            if (!passengers.Any())
                return false;

            return true;
        }
    }
}