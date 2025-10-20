using MediatR;
using SkyReserve.Application.Booking.DTOS;
using SkyReserve.Application.Booking.Queries.Models;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Booking.Queries.Handlers
{
    public class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, BookingDto?>
    {
        private readonly IBookingRepository _bookingRepository;

        public GetBookingByIdQueryHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<BookingDto?> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
        {
            return await _bookingRepository.GetByIdAsync(request.BookingId);
        }
    }
}