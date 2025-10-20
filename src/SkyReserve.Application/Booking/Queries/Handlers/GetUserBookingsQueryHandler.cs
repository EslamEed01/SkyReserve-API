using MediatR;
using SkyReserve.Application.Booking.DTOS;
using SkyReserve.Application.Booking.Queries.Models;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Booking.Queries.Handlers
{
    public class GetUserBookingsQueryHandler : IRequestHandler<GetUserBookingsQuery, IEnumerable<BookingDto>>
    {
        private readonly IBookingRepository _bookingRepository;

        public GetUserBookingsQueryHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<IEnumerable<BookingDto>> Handle(GetUserBookingsQuery request, CancellationToken cancellationToken)
        {
            return await _bookingRepository.GetByUserIdAsync(request.UserId);
        }
    }
}