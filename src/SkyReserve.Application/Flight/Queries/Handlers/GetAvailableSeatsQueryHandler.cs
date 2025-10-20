using MediatR;
using SkyReserve.Application.Flight.Queries.Models;
using SkyReserve.Application.Interfaces;

namespace SkyReserve.Application.Flight.Queries.Handlers
{
    public class GetAvailableSeatsQueryHandler : IRequestHandler<GetAvailableSeatsQuery, int>
    {
        private readonly IFlightRepository _flightRepository;

        public GetAvailableSeatsQueryHandler(IFlightRepository flightRepository)
        {
            _flightRepository = flightRepository;
        }

        public async Task<int> Handle(GetAvailableSeatsQuery request, CancellationToken cancellationToken)
        {
            return await _flightRepository.GetAvailableSeatsAsync(request.FlightId);
        }
    }
}