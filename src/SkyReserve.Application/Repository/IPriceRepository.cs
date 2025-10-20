using SkyReserve.Application.DTOs.Price.DTOs;

namespace SkyReserve.Application.Repository
{
    public interface IPriceRepository
    {
        Task<PriceDto> CreateAsync(CreatePriceDto price);
        Task<PriceDto?> GetByIdAsync(int id);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<PriceDto>> GetByFlightIdAsync(int flightId);
        Task<PriceDto?> GetActiveByFlightAndFareClassAsync(int flightId, string fareClass, DateTime bookingDate);
        Task<IEnumerable<PriceDto>> GetActivePricesAsync(int flightId, DateTime bookingDate);
    }
}