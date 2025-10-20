using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using SkyReserve.Application.Flight.DTOS;
using SkyReserve.Application.Interfaces;
using System.Data;

namespace SkyReserve.Infrastructure.Repository.implementation
{
    public class FlightRepository : IFlightRepository
    {
        private readonly string _connectionString;

        public FlightRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

        public async Task<FlightDto> CreateAsync(CreateFlightDto flight)
        {
            const string sql = @"
                INSERT INTO ""Flights"" (""FlightNumber"", ""DepartureAirportId"", ""ArrivalAirportId"", 
                    ""DepartureTime"", ""ArrivalTime"", ""AircraftModel"", ""Status"", ""CreatedAt"", ""UpdatedAt"")
                VALUES (@FlightNumber, @DepartureAirportId, @ArrivalAirportId, @DepartureTime, 
                    @ArrivalTime, @AircraftModel, @Status, @CreatedAt, @UpdatedAt)
                RETURNING ""FlightId"";";

            using var connection = CreateConnection();
            var now = DateTime.UtcNow;

            var flightId = await connection.QuerySingleAsync<int>(sql, new
            {
                flight.FlightNumber,
                flight.DepartureAirportId,
                flight.ArrivalAirportId,
                flight.DepartureTime,
                flight.ArrivalTime,
                flight.AircraftModel,
                flight.Status,
                CreatedAt = now,
                UpdatedAt = now
            });

            return await GetByIdAsync(flightId) ?? throw new InvalidOperationException("Failed to retrieve created flight");
        }

        public async Task<FlightDto> UpdateAsync(UpdateFlightDto flight)
        {
            var setParts = new List<string>();
            var parameters = new DynamicParameters();
            parameters.Add("FlightId", flight.FlightId);
            parameters.Add("UpdatedAt", DateTime.UtcNow);

            if (flight.FlightNumber != null)
            {
                setParts.Add(@"""FlightNumber"" = @FlightNumber");
                parameters.Add("FlightNumber", flight.FlightNumber);
            }
            if (flight.DepartureAirportId.HasValue)
            {
                setParts.Add(@"""DepartureAirportId"" = @DepartureAirportId");
                parameters.Add("DepartureAirportId", flight.DepartureAirportId);
            }
            if (flight.ArrivalAirportId.HasValue)
            {
                setParts.Add(@"""ArrivalAirportId"" = @ArrivalAirportId");
                parameters.Add("ArrivalAirportId", flight.ArrivalAirportId);
            }
            if (flight.DepartureTime.HasValue)
            {
                setParts.Add(@"""DepartureTime"" = @DepartureTime");
                parameters.Add("DepartureTime", flight.DepartureTime);
            }
            if (flight.ArrivalTime.HasValue)
            {
                setParts.Add(@"""ArrivalTime"" = @ArrivalTime");
                parameters.Add("ArrivalTime", flight.ArrivalTime);
            }
            if (flight.AircraftModel != null)
            {
                setParts.Add(@"""AircraftModel"" = @AircraftModel");
                parameters.Add("AircraftModel", flight.AircraftModel);
            }
            if (flight.Status != null)
            {
                setParts.Add(@"""Status"" = @Status");
                parameters.Add("Status", flight.Status);
            }

            if (!setParts.Any())
            {
                throw new ArgumentException("No fields to update");
            }

            setParts.Add(@"""UpdatedAt"" = @UpdatedAt");

            var sql = $@"UPDATE ""Flights"" SET {string.Join(", ", setParts)} WHERE ""FlightId"" = @FlightId";

            using var connection = CreateConnection();
            await connection.ExecuteAsync(sql, parameters);

            return await GetByIdAsync(flight.FlightId) ?? throw new InvalidOperationException("Failed to retrieve updated flight");
        }

        public async Task<bool> DeleteAsync(int flightId)
        {
            const string sql = @"DELETE FROM ""Flights"" WHERE ""FlightId"" = @FlightId";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(sql, new { FlightId = flightId });
            return affectedRows > 0;
        }

        public async Task<FlightDto?> GetByIdAsync(int flightId)
        {
            const string sql = @"
                SELECT f.""FlightId"", f.""FlightNumber"", f.""DepartureAirportId"", f.""ArrivalAirportId"",
                       f.""DepartureTime"", f.""ArrivalTime"", f.""AircraftModel"", f.""Status"",
                       f.""CreatedAt"", f.""UpdatedAt"",
                       da.""Code"" as DepartureAirportCode, da.""Name"" as DepartureAirportName,
                       aa.""Code"" as ArrivalAirportCode, aa.""Name"" as ArrivalAirportName
                FROM ""Flights"" f
                INNER JOIN ""Airports"" da ON f.""DepartureAirportId"" = da.""AirportId""
                INNER JOIN ""Airports"" aa ON f.""ArrivalAirportId"" = aa.""AirportId""
                WHERE f.""FlightId"" = @FlightId";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<FlightDto>(sql, new { FlightId = flightId });
        }

        public async Task<IEnumerable<FlightDto>> GetAllAsync(int pageNumber, int pageSize, string? status = null,
            int? departureAirportId = null, int? arrivalAirportId = null, DateTime? departureDate = null)
        {
            var whereClauses = new List<string>();
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(status))
            {
                whereClauses.Add(@"f.""Status"" = @Status");
                parameters.Add("Status", status);
            }

            if (departureAirportId.HasValue)
            {
                whereClauses.Add(@"f.""DepartureAirportId"" = @DepartureAirportId");
                parameters.Add("DepartureAirportId", departureAirportId);
            }

            if (arrivalAirportId.HasValue)
            {
                whereClauses.Add(@"f.""ArrivalAirportId"" = @ArrivalAirportId");
                parameters.Add("ArrivalAirportId", arrivalAirportId);
            }

            if (departureDate.HasValue)
            {
                whereClauses.Add(@"DATE(f.""DepartureTime"") = @DepartureDate");
                parameters.Add("DepartureDate", departureDate.Value.Date);
            }

            var whereClause = whereClauses.Any() ? "WHERE " + string.Join(" AND ", whereClauses) : "";

            parameters.Add("Offset", (pageNumber - 1) * pageSize);
            parameters.Add("PageSize", pageSize);

            var sql = $@"
                SELECT f.""FlightId"", f.""FlightNumber"", f.""DepartureAirportId"", f.""ArrivalAirportId"",
                       f.""DepartureTime"", f.""ArrivalTime"", f.""AircraftModel"", f.""Status"",
                       f.""CreatedAt"", f.""UpdatedAt"",
                       da.""Code"" as DepartureAirportCode, da.""Name"" as DepartureAirportName,
                       aa.""Code"" as ArrivalAirportCode, aa.""Name"" as ArrivalAirportName
                FROM ""Flights"" f
                INNER JOIN ""Airports"" da ON f.""DepartureAirportId"" = da.""AirportId""
                INNER JOIN ""Airports"" aa ON f.""ArrivalAirportId"" = aa.""AirportId""
                {whereClause}
                ORDER BY f.""DepartureTime""
                OFFSET @Offset LIMIT @PageSize";

            using var connection = CreateConnection();
            return await connection.QueryAsync<FlightDto>(sql, parameters);
        }

        public async Task<IEnumerable<FlightDto>> GetByRouteAsync(int departureAirportId, int arrivalAirportId, DateTime departureDate, int pageNumber, int pageSize)
        {
            const string sql = @"
                SELECT f.""FlightId"", f.""FlightNumber"", f.""DepartureAirportId"", f.""ArrivalAirportId"",
                       f.""DepartureTime"", f.""ArrivalTime"", f.""AircraftModel"", f.""Status"",
                       f.""CreatedAt"", f.""UpdatedAt"",
                       da.""Code"" as DepartureAirportCode, da.""Name"" as DepartureAirportName,
                       aa.""Code"" as ArrivalAirportCode, aa.""Name"" as ArrivalAirportName
                FROM ""Flights"" f
                INNER JOIN ""Airports"" da ON f.""DepartureAirportId"" = da.""AirportId""
                INNER JOIN ""Airports"" aa ON f.""ArrivalAirportId"" = aa.""AirportId""
                WHERE f.""DepartureAirportId"" = @DepartureAirportId 
                  AND f.""ArrivalAirportId"" = @ArrivalAirportId
                  AND DATE(f.""DepartureTime"") = @DepartureDate
                ORDER BY f.""DepartureTime""
                OFFSET @Offset LIMIT @PageSize";

            using var connection = CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("DepartureAirportId", departureAirportId);
            parameters.Add("ArrivalAirportId", arrivalAirportId);
            parameters.Add("DepartureDate", departureDate.Date);
            parameters.Add("Offset", (pageNumber - 1) * pageSize);
            parameters.Add("PageSize", pageSize);

            return await connection.QueryAsync<FlightDto>(sql, parameters);
        }

        public async Task<bool> ExistsAsync(int flightId)
        {
            const string sql = @"SELECT COUNT(1) FROM ""Flights"" WHERE ""FlightId"" = @FlightId";

            using var connection = CreateConnection();
            var count = await connection.QuerySingleAsync<int>(sql, new { FlightId = flightId });
            return count > 0;
        }

        public async Task<int> GetAvailableSeatsAsync(int flightId)
        {
            const string sql = @"SELECT ""AvailableSeats"" FROM ""Flights"" WHERE ""FlightId"" = @FlightId";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<int>(sql, new { FlightId = flightId });
        }

        public async Task<int> GetTotalSeatsAsync(int flightId)
        {
            const string sql = @"SELECT ""TotalSeats"" FROM ""Flights"" WHERE ""FlightId"" = @FlightId";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<int>(sql, new { FlightId = flightId });
        }

        public async Task<bool> UpdateAvailableSeatsAsync(int flightId, int seatChange)
        {
            const string sql = @"
                UPDATE ""Flights"" 
                SET ""AvailableSeats"" = ""AvailableSeats"" + @SeatChange, ""UpdatedAt"" = @UpdatedAt
                WHERE ""FlightId"" = @FlightId AND ""AvailableSeats"" + @SeatChange >= 0";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(sql, new
            {
                FlightId = flightId,
                SeatChange = seatChange,
                UpdatedAt = DateTime.UtcNow
            });

            return affectedRows > 0;
        }

        public async Task<bool> HasAvailableSeatsAsync(int flightId, int requiredSeats)
        {
            const string sql = @"SELECT ""AvailableSeats"" FROM ""Flights"" WHERE ""FlightId"" = @FlightId";

            using var connection = CreateConnection();
            var availableSeats = await connection.QuerySingleOrDefaultAsync<int>(sql, new { FlightId = flightId });
            return availableSeats >= requiredSeats;
        }

        public async Task<bool> FlightNumberExistsAsync(string flightNumber, int? excludeFlightId = null)
        {
            string sql = @"SELECT COUNT(1) FROM ""Flights"" WHERE ""FlightNumber"" = @FlightNumber";
            var parameters = new DynamicParameters();
            parameters.Add("FlightNumber", flightNumber);

            if (excludeFlightId.HasValue)
            {
                sql += @" AND ""FlightId"" != @ExcludeFlightId";
                parameters.Add("ExcludeFlightId", excludeFlightId.Value);
            }

            using var connection = CreateConnection();
            var count = await connection.QuerySingleAsync<int>(sql, parameters);
            return count > 0;
        }
    }
}
