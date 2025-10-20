using MediatR;
using SkyReserve.Application.Flight.Queries.Models;
using SkyReserve.Application.Interfaces;

namespace SkyReserve.Application.Flight.Queries.Handlers
{
    public class CheckAvailableSeatsQueryHandler : IRequestHandler<CheckAvailableSeatsQuery, bool>
    {
        private readonly IFlightRepository _flightRepository;

        public CheckAvailableSeatsQueryHandler(IFlightRepository flightRepository)
        {
            _flightRepository = flightRepository;
        }

        public async Task<bool> Handle(CheckAvailableSeatsQuery request, CancellationToken cancellationToken)
        {
            return await _flightRepository.HasAvailableSeatsAsync(request.FlightId, request.RequiredSeats);
        }
    }
}