using MediatR;
using SkyReserve.Application.Flight.DTOS;
using SkyReserve.Application.Flight.Queries.Models;
using SkyReserve.Application.Interfaces;

namespace SkyReserve.Application.Flight.Queries.Handlers
{
    public class GetFlightsByRouteQueryHandler : IRequestHandler<GetFlightsByRouteQuery, IEnumerable<FlightDto>>
    {
        private readonly IFlightRepository _flightRepository;

        public GetFlightsByRouteQueryHandler(IFlightRepository flightRepository)
        {
            _flightRepository = flightRepository;
        }

        public async Task<IEnumerable<FlightDto>> Handle(GetFlightsByRouteQuery request, CancellationToken cancellationToken)
        {
            return await _flightRepository.GetByRouteAsync(
                request.DepartureAirportId,
                request.ArrivalAirportId,
                request.DepartureDate,
                request.PageNumber,
                request.PageSize);
        }
    }
}
