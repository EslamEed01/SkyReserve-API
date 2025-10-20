using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkyReserve.Application.Flight.DTOS;
using SkyReserve.Application.Interfaces;
using SkyReserve.Application.Settings;

namespace SkyReserve.Application.Services
{
    public class ElasticsearchService : IElasticsearchService
    {
        private readonly ElasticsearchClient _client;
        private readonly ElasticsearchSettings _settings;
        private readonly ILogger<ElasticsearchService> _logger;

        public ElasticsearchService(
            ElasticsearchClient client,
            IOptions<ElasticsearchSettings> settings,
            ILogger<ElasticsearchService> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> IndexFlightAsync(FlightDto flight)
        {
            if (flight == null)
            {
                _logger.LogWarning("Attempted to index null flight");
                return false;
            }

            try
            {
                var response = await _client.IndexAsync(flight, idx => idx
                    .Index(_settings.DefaultIndex)
                    .Id(flight.FlightId.ToString()));

                if (!response.IsValidResponse)
                {
                    _logger.LogWarning("Invalid response when indexing flight {FlightId}: {Error}",
                        flight.FlightId, response.DebugInformation);
                }

                return response.IsValidResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing flight {FlightId}", flight.FlightId);
                return false;
            }
        }

        public async Task<bool> UpdateFlightAsync(FlightDto flight)
        {
            if (flight == null)
            {
                _logger.LogWarning("Attempted to update null flight");
                return false;
            }

            try
            {
                var response = await _client.UpdateAsync<FlightDto, FlightDto>(
                    IndexName.Parse(_settings.DefaultIndex, null),
                    flight.FlightId.ToString(),
                    u => u.Doc(flight).DocAsUpsert(true));

                if (!response.IsValidResponse)
                {
                    _logger.LogWarning("Invalid response when updating flight {FlightId}: {Error}",
                        flight.FlightId, response.DebugInformation);
                }

                return response.IsValidResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating flight {FlightId}", flight.FlightId);
                return false;
            }
        }

        public async Task<bool> DeleteFlightAsync(int flightId)
        {
            if (flightId <= 0)
            {
                _logger.LogWarning("Attempted to delete flight with invalid ID: {FlightId}", flightId);
                return false;
            }

            try
            {
                var response = await _client.DeleteAsync(
                    IndexName.Parse(_settings.DefaultIndex, null),
                    flightId.ToString());

                if (!response.IsValidResponse)
                {
                    _logger.LogWarning("Invalid response when deleting flight {FlightId}: {Error}",
                        flightId, response.DebugInformation);
                }

                return response.IsValidResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting flight {FlightId}", flightId);
                return false;
            }
        }

        public async Task<FlightDto?> GetFlightAsync(int flightId)
        {
            if (flightId <= 0)
            {
                _logger.LogWarning("Attempted to get flight with invalid ID: {FlightId}", flightId);
                return null;
            }

            try
            {
                var response = await _client.GetAsync<FlightDto>(
                    IndexName.Parse(_settings.DefaultIndex, null),
                    flightId.ToString());

                if (!response.IsValidResponse)
                {
                    _logger.LogWarning("Invalid response when getting flight {FlightId}: {Error}",
                        flightId, response.DebugInformation);
                }

                return response.IsValidResponse ? response.Source : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting flight {FlightId}", flightId);
                return null;
            }
        }

        public async Task<IEnumerable<FlightDto>> SearchFlightsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                _logger.LogWarning("Attempted to search with null or empty query");
                return Enumerable.Empty<FlightDto>();
            }

            try
            {
                var response = await _client.SearchAsync<FlightDto>(s => s
                    .Index(_settings.DefaultIndex)
                    .Query(q => q
                        .QueryString(qs => qs.Query(query))));

                if (!response.IsValidResponse)
                {
                    _logger.LogWarning("Invalid response when searching flights with query '{Query}': {Error}",
                        query, response.DebugInformation);
                }

                return response.IsValidResponse ? response.Documents : Enumerable.Empty<FlightDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching flights with query: {Query}", query);
                return Enumerable.Empty<FlightDto>();
            }
        }

        public async Task<bool> DeleteIndexAsync(string indexName)
        {
            if (string.IsNullOrWhiteSpace(indexName))
            {
                _logger.LogWarning("Attempted to delete index with null or empty name");
                return false;
            }

            try
            {
                var response = await _client.Indices.DeleteAsync(indexName);

                if (!response.IsValidResponse)
                {
                    _logger.LogWarning("Invalid response when deleting index '{IndexName}': {Error}",
                        indexName, response.DebugInformation);
                }

                return response.IsValidResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting index '{IndexName}'", indexName);
                return false;
            }
        }

        public async Task<bool> EnsureIndexExistsAsync()
        {
            try
            {
                var indexExistsResponse = await _client.Indices.ExistsAsync(_settings.DefaultIndex);

                if (!indexExistsResponse.Exists)
                {
                    var createIndexResponse = await _client.Indices.CreateAsync(_settings.DefaultIndex, c => c
                        .Mappings(m => m
                            .Properties<FlightDto>(p => p
                                .Keyword(f => f.FlightNumber)
                                .IntegerNumber(f => f.DepartureAirportId)
                                .IntegerNumber(f => f.ArrivalAirportId)
                                .Date(f => f.DepartureTime)
                                .Date(f => f.ArrivalTime)
                                .Text(f => f.AircraftModel)
                                .Keyword(f => f.Status)
                            )
                        )
                    );

                    return createIndexResponse.IsValidResponse;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring index exists");
                return false;
            }
        }
    }
}