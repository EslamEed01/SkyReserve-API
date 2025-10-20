using MediatR;
using SkyReserve.Application.Passenger.DTOS;
using SkyReserve.Application.Passenger.Queries.Models;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Passenger.Queries.Handlers
{
    public class GetPassengerByIdQueryHandler : IRequestHandler<GetPassengerByIdQuery, PassengerDto?>
    {
        private readonly IPassengerRepository _passengerRepository;

        public GetPassengerByIdQueryHandler(IPassengerRepository passengerRepository)
        {
            _passengerRepository = passengerRepository;
        }

        public async Task<PassengerDto?> Handle(GetPassengerByIdQuery request, CancellationToken cancellationToken)
        {
            return await _passengerRepository.GetByIdAsync(request.PassengerId);
        }
    }
}