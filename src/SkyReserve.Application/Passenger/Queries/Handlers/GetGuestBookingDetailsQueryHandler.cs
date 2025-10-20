using MediatR;
using SkyReserve.Application.Passenger.DTOS;
using SkyReserve.Application.Passenger.Queries.Models;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Passenger.Queries.Handlers
{
    public class GetGuestBookingDetailsQueryHandler : IRequestHandler<GetGuestBookingDetailsQuery, GuestBookingDetailsDto?>
    {
        private readonly IPassengerRepository _passengerRepository;

        public GetGuestBookingDetailsQueryHandler(IPassengerRepository passengerRepository)
        {
            _passengerRepository = passengerRepository;
        }

        public async Task<GuestBookingDetailsDto?> Handle(GetGuestBookingDetailsQuery request, CancellationToken cancellationToken)
        {
            return await _passengerRepository.GetGuestBookingDetailsAsync(request.BookingRef, request.LastName);
        }
    }
}