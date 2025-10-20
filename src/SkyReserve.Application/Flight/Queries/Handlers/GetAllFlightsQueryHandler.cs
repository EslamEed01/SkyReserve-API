using MediatR;
using SkyReserve.Application.Flight.DTOS;
using SkyReserve.Application.Flight.Queries.Models;
using SkyReserve.Application.Interfaces;

namespace SkyReserve.Application.Flight.Queries.Handlers
{
    public class GetAllFlightsQueryHandler : IRequestHandler<GetAllFlightsQuery, IEnumerable<FlightDto>>
    {
        private readonly IFlightRepository _flightRepository;

        public GetAllFlightsQueryHandler(IFlightRepository flightRepository)
        {
            _flightRepository = flightRepository;
        }

        public async Task<IEnumerable<FlightDto>> Handle(GetAllFlightsQuery request, CancellationToken cancellationToken)
        {
            return await _flightRepository.GetAllAsync(
                request.PageNumber,
                request.PageSize,
                request.Status,
                request.DepartureAirportId,
                request.ArrivalAirportId,
                request.DepartureDate);
        }
    }
}
