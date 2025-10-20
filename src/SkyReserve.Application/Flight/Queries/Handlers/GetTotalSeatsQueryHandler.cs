using MediatR;
using SkyReserve.Application.Flight.Queries.Models;
using SkyReserve.Application.Interfaces;

namespace SkyReserve.Application.Flight.Queries.Handlers
{
    public class GetTotalSeatsQueryHandler : IRequestHandler<GetTotalSeatsQuery, int>
    {
        private readonly IFlightRepository _flightRepository;

        public GetTotalSeatsQueryHandler(IFlightRepository flightRepository)
        {
            _flightRepository = flightRepository;
        }

        public async Task<int> Handle(GetTotalSeatsQuery request, CancellationToken cancellationToken)
        {
            return await _flightRepository.GetTotalSeatsAsync(request.FlightId);
        }
    }
}