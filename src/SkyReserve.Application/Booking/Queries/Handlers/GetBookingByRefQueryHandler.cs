using MediatR;
using SkyReserve.Application.Booking.DTOS;
using SkyReserve.Application.Booking.Queries.Models;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Booking.Queries.Handlers
{
    public class GetBookingByRefQueryHandler : IRequestHandler<GetBookingByRefQuery, BookingDto?>
    {
        private readonly IBookingRepository _bookingRepository;

        public GetBookingByRefQueryHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<BookingDto?> Handle(GetBookingByRefQuery request, CancellationToken cancellationToken)
        {
            return await _bookingRepository.GetByBookingRefAsync(request.BookingRef);
        }
    }
}