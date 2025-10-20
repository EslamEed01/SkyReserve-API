using MediatR;
using SkyReserve.Application.Booking.DTOS;
using SkyReserve.Application.Booking.Queries.Models;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Booking.Queries.Handlers
{
    public class GetFlightBookingsQueryHandler : IRequestHandler<GetFlightBookingsQuery, IEnumerable<BookingDto>>
    {
        private readonly IBookingRepository _bookingRepository;

        public GetFlightBookingsQueryHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<IEnumerable<BookingDto>> Handle(GetFlightBookingsQuery request, CancellationToken cancellationToken)
        {
            return await _bookingRepository.GetByFlightIdAsync(request.FlightId);
        }
    }
}