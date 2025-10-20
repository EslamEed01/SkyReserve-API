using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SkyReserve.Application.Flight.DTOS;
using SkyReserve.Application.Interfaces;
using SkyReserve.Application.Settings;
using StackExchange.Redis;

namespace SkyReserve.Application.Services
{
    public class RedisService : IRedisService
    {
        private readonly IDatabase _database;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly RedisSettings _settings;
        private readonly ILogger<RedisService> _logger;

        public RedisService(
            IConnectionMultiplexer connectionMultiplexer,
            IOptions<RedisSettings> settings,
            ILogger<RedisService> logger)
        {
            _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
            _database = connectionMultiplexer.GetDatabase(settings?.Value?.Database ?? 0);
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            if (string.IsNullOrWhiteSpace(key)) return null;

            try
            {
                var value = await _database.StringGetAsync(GetFullKey(key));
                return value.HasValue ? JsonConvert.DeserializeObject<T>(value!) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value for key {Key}", key);
                return null;
            }
        }

        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            if (string.IsNullOrWhiteSpace(key) || value == null) return false;

            try
            {
                var serializedValue = JsonConvert.SerializeObject(value);
                var expirationTime = expiration ?? _settings.DefaultExpiration;
                return await _database.StringSetAsync(GetFullKey(key), serializedValue, expirationTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value for key {Key}", key);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return false;

            try
            {
                return await _database.KeyDeleteAsync(GetFullKey(key));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting key {Key}", key);
                return false;
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return false;

            try
            {
                return await _database.KeyExistsAsync(GetFullKey(key));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence for key {Key}", key);
                return false;
            }
        }

        public async Task<bool> CacheFlightAsync(FlightDto flight, TimeSpan? expiration = null)
        {
            if (flight == null) return false;
            return await SetAsync($"flight:{flight.FlightId}", flight, expiration);
        }

        public async Task<FlightDto?> GetCachedFlightAsync(int flightId)
        {
            if (flightId <= 0) return null;
            return await GetAsync<FlightDto>($"flight:{flightId}");
        }

        public async Task<bool> RemoveCachedFlightAsync(int flightId)
        {
            if (flightId <= 0) return false;
            return await DeleteAsync($"flight:{flightId}");
        }

        public async Task<bool> CacheFlightSearchResultsAsync(string searchQuery, IEnumerable<FlightDto> flights, TimeSpan? expiration = null)
        {
            if (string.IsNullOrWhiteSpace(searchQuery) || flights == null) return false;

            var key = $"search:{searchQuery.ToLowerInvariant()}";
            var searchExpiration = expiration ?? TimeSpan.FromMinutes(15);
            return await SetAsync(key, flights.ToList(), searchExpiration);
        }

        public async Task<IEnumerable<FlightDto>?> GetCachedFlightSearchResultsAsync(string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery)) return null;

            var key = $"search:{searchQuery.ToLowerInvariant()}";
            var result = await GetAsync<List<FlightDto>>(key);
            return result ?? Enumerable.Empty<FlightDto>();
        }

        public async Task<bool> DeleteByPatternAsync(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern)) return false;

            try
            {
                var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());
                var keys = server.Keys(pattern: GetFullKey(pattern)).ToArray();

                if (keys.Length == 0) return true;

                var result = await _database.KeyDeleteAsync(keys);
                _logger.LogInformation("Deleted {Count} keys matching pattern {Pattern}", result, pattern);
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting keys by pattern {Pattern}", pattern);
                return false;
            }
        }

        public async Task<Dictionary<string, T?>> GetMultipleAsync<T>(IEnumerable<string> keys) where T : class
        {
            var result = new Dictionary<string, T?>();
            if (keys?.Any() != true) return result;

            try
            {
                var keyArray = keys.ToArray();
                var fullKeys = keyArray.Select(GetFullKey).ToArray();
                var redisKeys = fullKeys.Select(k => (StackExchange.Redis.RedisKey)k).ToArray();
                var values = await _database.StringGetAsync(redisKeys);

                for (int i = 0; i < keyArray.Length; i++)
                {
                    var originalKey = keyArray[i];
                    var value = values[i];

                    result[originalKey] = value.HasValue
                        ? DeserializeSafely<T>(value!, originalKey)
                        : null;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting multiple values");
                return result;
            }
        }

        private T? DeserializeSafely<T>(string value, string key) where T : class
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing value for key {Key}", key);
                return null;
            }
        }

        private string GetFullKey(string key) => $"{_settings.KeyPrefix}{key}";
    }
}