using MediatR;
using SkyReserve.Application.Flight.Commands.Models;
using SkyReserve.Application.Interfaces;

namespace SkyReserve.Application.Flight.Commands.Handlers
{
    public class DeleteFlightCommandHandler : IRequestHandler<DeleteFlightCommand, bool>
    {
        private readonly IFlightRepository _flightRepository;
        private readonly IElasticsearchService _elasticsearchService;
        private readonly IRedisService _redisService;

        public DeleteFlightCommandHandler(
            IFlightRepository flightRepository,
            IElasticsearchService elasticsearchService,
            IRedisService redisService)
        {
            _flightRepository = flightRepository;
            _elasticsearchService = elasticsearchService;
            _redisService = redisService;
        }

        public async Task<bool> Handle(DeleteFlightCommand request, CancellationToken cancellationToken)
        {
            if (!await _flightRepository.ExistsAsync(request.FlightId))
            {
                throw new KeyNotFoundException($"Flight with ID {request.FlightId} not found.");
            }

            var dbResult = await _flightRepository.DeleteAsync(request.FlightId);

            if (dbResult)
            {
                await _elasticsearchService.DeleteFlightAsync(request.FlightId);

                await _redisService.RemoveCachedFlightAsync(request.FlightId);

                await _redisService.DeleteByPatternAsync("search:*");
            }

            return dbResult;
        }
    }
}
