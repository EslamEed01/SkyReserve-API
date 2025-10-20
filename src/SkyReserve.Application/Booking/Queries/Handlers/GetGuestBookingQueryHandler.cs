using MediatR;
using SkyReserve.Application.Booking.DTOS;
using SkyReserve.Application.Booking.Queries.Models;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Booking.Queries.Handlers
{
    public class GetGuestBookingQueryHandler : IRequestHandler<GetGuestBookingQuery, BookingDto?>
    {
        private readonly IBookingRepository _bookingRepository;

        public GetGuestBookingQueryHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<BookingDto?> Handle(GetGuestBookingQuery request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByBookingRefAsync(request.BookingRef);

            if (booking == null)
                return null;

            if (!string.IsNullOrEmpty(booking.UserId))
                return null;

            var hasMatchingPassenger = booking.Passengers?.Any(p =>
                string.Equals(p.LastName, request.LastName, StringComparison.OrdinalIgnoreCase)) ?? false;

            if (!hasMatchingPassenger)
                return null;

            return booking;
        }
    }
}