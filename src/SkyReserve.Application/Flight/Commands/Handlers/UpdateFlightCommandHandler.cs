using MediatR;
using SkyReserve.Application.Flight.Commands.Models;
using SkyReserve.Application.Flight.DTOS;
using SkyReserve.Application.Interfaces;

namespace SkyReserve.Application.Flight.Commands.Handlers
{
    public class UpdateFlightCommandHandler : IRequestHandler<UpdateFlightCommand, FlightDto>
    {
        private readonly IFlightRepository _flightRepository;
        private readonly IElasticsearchService _elasticsearchService;

        public UpdateFlightCommandHandler(IFlightRepository flightRepository, IElasticsearchService elasticsearchService)
        {
            _flightRepository = flightRepository;
            _elasticsearchService = elasticsearchService;
        }

        public async Task<FlightDto> Handle(UpdateFlightCommand request, CancellationToken cancellationToken)
        {
            var updateFlightDto = new UpdateFlightDto
            {
                FlightId = request.FlightId,
                FlightNumber = request.FlightNumber,
                DepartureAirportId = request.DepartureAirportId,
                ArrivalAirportId = request.ArrivalAirportId,
                DepartureTime = request.DepartureTime,
                ArrivalTime = request.ArrivalTime,
                AircraftModel = request.AircraftModel,
                Status = request.Status
            };

            var flight = await _flightRepository.UpdateAsync(updateFlightDto);

            await _elasticsearchService.UpdateFlightAsync(flight);

            return flight;
        }
    }
}
