using MediatR;
using SkyReserve.Application.Flight.Commands.Models;
using SkyReserve.Application.Interfaces;

namespace SkyReserve.Application.Flight.Commands.Handlers
{
    public class UpdateFlightSeatsCommandHandler : IRequestHandler<UpdateFlightSeatsCommand, bool>
    {
        private readonly IFlightRepository _flightRepository;
        private readonly IElasticsearchService _elasticsearchService;
        private readonly IRedisService _redisService;

        public UpdateFlightSeatsCommandHandler(
            IFlightRepository flightRepository,
            IElasticsearchService elasticsearchService,
            IRedisService redisService)
        {
            _flightRepository = flightRepository;
            _elasticsearchService = elasticsearchService;
            _redisService = redisService;
        }

        public async Task<bool> Handle(UpdateFlightSeatsCommand request, CancellationToken cancellationToken)
        {
            var result = await _flightRepository.UpdateAvailableSeatsAsync(request.FlightId, request.SeatChange);

            if (result)
            {
                var updatedFlight = await _flightRepository.GetByIdAsync(request.FlightId);
                if (updatedFlight != null)
                {
                    await _elasticsearchService.UpdateFlightAsync(updatedFlight);

                    await _redisService.CacheFlightAsync(updatedFlight);

                    await _redisService.DeleteByPatternAsync("search:*");
                }
            }

            return result;
        }
    }
}