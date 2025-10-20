public interface IElasticsearchService
{
    Task<bool> IndexFlightAsync(FlightDto flight);
    Task<bool> UpdateFlightAsync(FlightDto flight);
    Task<bool> DeleteFlightAsync(int flightId);
    Task<FlightDto?> GetFlightAsync(int flightId);
    Task<IEnumerable<FlightDto>> SearchFlightsAsync(string query);
    Task<bool> EnsureIndexExistsAsync();
}