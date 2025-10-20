using MediatR;
using SkyReserve.Application.Passenger.DTOS;
using SkyReserve.Application.Passenger.Queries.Models;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Passenger.Queries.Handlers
{
    public class GetAllPassengersQueryHandler : IRequestHandler<GetAllPassengersQuery, IEnumerable<PassengerDto>>
    {
        private readonly IPassengerRepository _passengerRepository;

        public GetAllPassengersQueryHandler(IPassengerRepository passengerRepository)
        {
            _passengerRepository = passengerRepository;
        }

        public async Task<IEnumerable<PassengerDto>> Handle(GetAllPassengersQuery request, CancellationToken cancellationToken)
        {
            return await _passengerRepository.GetAllAsync(request.PageNumber, request.PageSize);
        }
    }
}