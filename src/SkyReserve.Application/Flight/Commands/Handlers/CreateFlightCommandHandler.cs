using MediatR;
using SkyReserve.Application.Flight.Commands.Models;
using SkyReserve.Application.Flight.DTOS;
using SkyReserve.Application.Interfaces;

namespace SkyReserve.Application.Flight.Commands.Handlers
{
    public class CreateFlightCommandHandler : IRequestHandler<CreateFlightCommand, FlightDto>
    {
        private readonly IFlightRepository _flightRepository;
        private readonly IElasticsearchService _elasticsearchService;
        private readonly IRedisService _redisService;

        public CreateFlightCommandHandler(
            IFlightRepository flightRepository,
            IElasticsearchService elasticsearchService,
            IRedisService redisService)
        {
            _flightRepository = flightRepository;
            _elasticsearchService = elasticsearchService;
            _redisService = redisService;
        }

        public async Task<FlightDto> Handle(CreateFlightCommand request, CancellationToken cancellationToken)
        {
            var createFlightDto = new CreateFlightDto
            {
                FlightNumber = request.FlightNumber,
                DepartureAirportId = request.DepartureAirportId,
                ArrivalAirportId = request.ArrivalAirportId,
                DepartureTime = request.DepartureTime,
                ArrivalTime = request.ArrivalTime,
                AircraftModel = request.AircraftModel,
                Status = request.Status
            };

            var flight = await _flightRepository.CreateAsync(createFlightDto);

            await _elasticsearchService.IndexFlightAsync(flight);

            await _redisService.CacheFlightAsync(flight);

            await _redisService.DeleteByPatternAsync("search:*");

            return flight;
        }
    }
}
