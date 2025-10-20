using MediatR;
using SkyReserve.Application.Passenger.DTOS;
using SkyReserve.Application.Passenger.Queries.Models;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Passenger.Queries.Handlers
{
    public class GetPassengersByBookingIdQueryHandler : IRequestHandler<GetPassengersByBookingIdQuery, IEnumerable<PassengerDto>>
    {
        private readonly IPassengerRepository _passengerRepository;

        public GetPassengersByBookingIdQueryHandler(IPassengerRepository passengerRepository)
        {
            _passengerRepository = passengerRepository;
        }

        public async Task<IEnumerable<PassengerDto>> Handle(GetPassengersByBookingIdQuery request, CancellationToken cancellationToken)
        {
            return await _passengerRepository.GetByBookingIdAsync(request.BookingId);
        }
    }
}