using SkyReserve.Application.Flight.DTOS;

namespace SkyReserve.Application.Interfaces
{
    public interface IRedisService
    {
        Task<T?> GetAsync<T>(string key) where T : class;
        Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
        Task<bool> DeleteAsync(string key);
        Task<bool> ExistsAsync(string key);
        Task<bool> CacheFlightAsync(FlightDto flight, TimeSpan? expiration = null);
        Task<FlightDto?> GetCachedFlightAsync(int flightId);
        Task<bool> RemoveCachedFlightAsync(int flightId);
        Task<bool> CacheFlightSearchResultsAsync(string searchQuery, IEnumerable<FlightDto> flights, TimeSpan? expiration = null);
        Task<IEnumerable<FlightDto>?> GetCachedFlightSearchResultsAsync(string searchQuery);
        Task<bool> DeleteByPatternAsync(string pattern);
        Task<Dictionary<string, T?>> GetMultipleAsync<T>(IEnumerable<string> keys) where T : class;
    }
}