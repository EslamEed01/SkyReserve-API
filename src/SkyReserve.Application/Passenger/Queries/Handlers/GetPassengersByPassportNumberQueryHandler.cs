using MediatR;
using SkyReserve.Application.Passenger.DTOS;
using SkyReserve.Application.Passenger.Queries.Models;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Passenger.Queries.Handlers
{
    public class GetPassengersByPassportNumberQueryHandler : IRequestHandler<GetPassengersByPassportNumberQuery, IEnumerable<PassengerDto>>
    {
        private readonly IPassengerRepository _passengerRepository;

        public GetPassengersByPassportNumberQueryHandler(IPassengerRepository passengerRepository)
        {
            _passengerRepository = passengerRepository;
        }

        public async Task<IEnumerable<PassengerDto>> Handle(GetPassengersByPassportNumberQuery request, CancellationToken cancellationToken)
        {
            return await _passengerRepository.GetByPassportNumberAsync(request.PassportNumber);
        }
    }
}