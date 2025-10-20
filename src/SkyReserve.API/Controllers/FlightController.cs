using MediatR;
using Microsoft.AspNetCore.Mvc;
using SkyReserve.Application.Flight.Commands.Models;
using SkyReserve.Application.Flight.Queries.Models;
using SkyReserve.Application.Interfaces;

namespace SkyReserve.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IElasticsearchService _elasticsearchService;
        private readonly ILogger<FlightController> _logger;

        public FlightController(
            IMediator mediator, 
            IElasticsearchService elasticsearchService,
            ILogger<FlightController> logger)
        {
            _mediator = mediator;
            _elasticsearchService = elasticsearchService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFlight(int id)
        {
            var query = new GetFlightByIdQuery { FlightId = id };
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetFlights([FromQuery] GetAllFlightsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("route")]
        public async Task<IActionResult> GetFlightsByRoute([FromQuery] GetFlightsByRouteQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFlight([FromBody] CreateFlightCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                
                try
                {
                    var indexed = await _elasticsearchService.IndexFlightAsync(result);
                    if (!indexed)
                    {
                        _logger.LogWarning("Failed to index flight {FlightId} in ElasticSearch", result.FlightId);
                    }
                    else
                    {
                        _logger.LogInformation("Successfully indexed flight {FlightId} in ElasticSearch", result.FlightId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error indexing flight {FlightId} in ElasticSearch", result.FlightId);
                }
                
                return CreatedAtAction(nameof(GetFlight), new { id = result.FlightId }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFlight(int id, [FromBody] UpdateFlightCommand command)
        {
            try
            {
                command.FlightId = id;
                var result = await _mediator.Send(command);
                try
                {
                    var updated = await _elasticsearchService.UpdateFlightAsync(result);
                    if (!updated)
                    {
                        _logger.LogWarning("Failed to update flight {FlightId} in ElasticSearch", result.FlightId);
                    }
                    else
                    {
                        _logger.LogInformation("Successfully updated flight {FlightId} in ElasticSearch", result.FlightId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating flight {FlightId} in ElasticSearch", result.FlightId);
                }
                
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFlight(int id)
        {
            try
            {
                var command = new DeleteFlightCommand { FlightId = id };
                var result = await _mediator.Send(command);

                if (result)
                {
                    try
                    {
                        var deleted = await _elasticsearchService.DeleteFlightAsync(id);
                        if (!deleted)
                        {
                            _logger.LogWarning("Failed to delete flight {FlightId} from ElasticSearch", id);
                        }
                        else
                        {
                            _logger.LogInformation("Successfully deleted flight {FlightId} from ElasticSearch", id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deleting flight {FlightId} from ElasticSearch", id);
                    }
                    
                    return NoContent();
                }

                return BadRequest("Failed to delete flight");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchFlights([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Query parameter is required");
            }

            try
            {
                var results = await _elasticsearchService.SearchFlightsAsync(query);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching flights with query: {Query}", query);
                return StatusCode(500, "An error occurred while searching flights");
            }
        }

        [HttpGet("elastic/{id}")]
        public async Task<IActionResult> GetFlightFromElastic(int id)
        {
            try
            {
                var flight = await _elasticsearchService.GetFlightAsync(id);
                
                if (flight == null)
                    return NotFound($"Flight with ID {id} not found in ElasticSearch");

                return Ok(flight);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting flight {FlightId} from ElasticSearch", id);
                return StatusCode(500, "An error occurred while retrieving flight from ElasticSearch");
            }
        }

        [HttpPost("reindex")]
        public async Task<IActionResult> ReindexFlights()
        {
            try
            {
                var query = new GetAllFlightsQuery { PageNumber = 1, PageSize = int.MaxValue };
                var flights = await _mediator.Send(query);
                
                var allIndexed = true;
                foreach (var flight in flights)
                {
                    var indexed = await _elasticsearchService.IndexFlightAsync(flight);
                    if (!indexed)
                    {
                        allIndexed = false;
                        _logger.LogWarning("Failed to reindex flight {FlightId}", flight.FlightId);
                    }
                }
                var success = allIndexed;
                
                if (success)
                {
                    _logger.LogInformation("Successfully reindexed {Count} flights", flights.Count());
                    return Ok(new { message = $"Successfully reindexed {flights.Count()} flights", count = flights.Count() });
                }
                else
                {
                    _logger.LogWarning("Failed to reindex flights");
                    return StatusCode(500, "Failed to reindex flights");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during flight reindexing");
                return StatusCode(500, "An error occurred during reindexing");
            }
        }

        [HttpPost("index/ensure")]
        public async Task<IActionResult> EnsureFlightIndex()
        {
            try
            {
                var success = await _elasticsearchService.EnsureIndexExistsAsync();
                
                if (success)
                {
                    return Ok(new { message = "Flight index exists or was created successfully" });
                }
                else
                {
                    return StatusCode(500, "Failed to ensure flight index exists");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring flight index exists");
                return StatusCode(500, "An error occurred while ensuring index exists");
            }
        }

        [HttpDelete("index")]
        public async Task<IActionResult> DeleteFlightIndex()
        {
            try
            {
                var success = await _elasticsearchService.DeleteIndexAsync("flights");
                
                if (success)
                {
                    _logger.LogInformation("Successfully deleted flight index");
                    return Ok(new { message = "Flight index deleted successfully" });
                }
                else
                {
                    return StatusCode(500, "Failed to delete flight index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting flight index");
                return StatusCode(500, "An error occurred while deleting index");
            }
        }

        [HttpGet("search/advanced")]
        public async Task<IActionResult> AdvancedSearchFlights(
            [FromQuery] string? query,
            [FromQuery] string? departureAirport,
            [FromQuery] string? arrivalAirport,
            [FromQuery] string? status,
            [FromQuery] DateTime? departureDate,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
        {
            try
            {
                var searchTerms = new List<string>();

                if (!string.IsNullOrWhiteSpace(query))
                    searchTerms.Add(query);

                if (!string.IsNullOrWhiteSpace(departureAirport))
                    searchTerms.Add($"departure_airport_code:{departureAirport} OR departure_airport_name:{departureAirport}");

                if (!string.IsNullOrWhiteSpace(arrivalAirport))
                    searchTerms.Add($"arrival_airport_code:{arrivalAirport} OR arrival_airport_name:{arrivalAirport}");

                if (!string.IsNullOrWhiteSpace(status))
                    searchTerms.Add($"status:{status}");

                if (departureDate.HasValue)
                    searchTerms.Add($"departure_time:[{departureDate.Value:yyyy-MM-dd} TO {departureDate.Value.AddDays(1):yyyy-MM-dd}]");

                var finalQuery = searchTerms.Count > 0 
                    ? string.Join(" AND ", searchTerms.Select(term => $"({term})"))
                    : "*";

                var results = await _elasticsearchService.SearchFlightsAsync(finalQuery);
                
                var pagedResults = results.Skip((page - 1) * size).Take(size);
                
                return Ok(new 
                { 
                    flights = pagedResults,
                    totalCount = results.Count(),
                    page,
                    size,
                    totalPages = (int)Math.Ceiling(results.Count() / (double)size)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in advanced flight search");
                return StatusCode(500, "An error occurred during advanced search");
            }
        }
    }
}
