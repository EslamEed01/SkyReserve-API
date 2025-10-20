using SkyReserve.Application.Flight.DTOS;

namespace SkyReserve.Application.Interfaces
{
    public interface IFlightRepository
    {
        Task<FlightDto> CreateAsync(CreateFlightDto flight);
        Task<FlightDto> UpdateAsync(UpdateFlightDto flight);
        Task<bool> DeleteAsync(int flightId);
        Task<FlightDto?> GetByIdAsync(int flightId);
        Task<IEnumerable<FlightDto>> GetAllAsync(int pageNumber, int pageSize, string? status = null,
            int? departureAirportId = null, int? arrivalAirportId = null, DateTime? departureDate = null);
       
        Task<bool> ExistsAsync(int flightId);
        Task<int> GetAvailableSeatsAsync(int flightId);
        Task<int> GetTotalSeatsAsync(int flightId);
        Task<bool> UpdateAvailableSeatsAsync(int flightId, int seatChange);
        Task<bool> HasAvailableSeatsAsync(int flightId, int requiredSeats);
        Task<bool> FlightNumberExistsAsync(string flightNumber, int? excludeFlightId = null);
        Task<IEnumerable<FlightDto>> GetByRouteAsync(int departureAirportId, int arrivalAirportId, DateTime departureDate, int pageNumber, int pageSize);
    }
}
