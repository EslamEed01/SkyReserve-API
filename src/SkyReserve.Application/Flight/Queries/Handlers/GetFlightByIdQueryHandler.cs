using MediatR;
using SkyReserve.Application.Flight.DTOS;
using SkyReserve.Application.Flight.Queries.Models;
using SkyReserve.Application.Interfaces;

namespace SkyReserve.Application.Flight.Queries.Handlers
{
    public class GetFlightByIdQueryHandler : IRequestHandler<GetFlightByIdQuery, FlightDto?>
    {
        private readonly IFlightRepository _flightRepository;

        public GetFlightByIdQueryHandler(IFlightRepository flightRepository)
        {
            _flightRepository = flightRepository;
        }

        public async Task<FlightDto?> Handle(GetFlightByIdQuery request, CancellationToken cancellationToken)
        {
            return await _flightRepository.GetByIdAsync(request.FlightId);
        }
    }
}
