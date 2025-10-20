using SkyReserve.Application.Passenger.DTOS;

namespace SkyReserve.Application.Repository
{
    public interface IPassengerRepository
    {
        Task<PassengerDto> CreateAsync(CreatePassengerDto passenger);
        Task<PassengerDto> UpdateAsync(UpdatePassengerDto passenger);
        Task<bool> DeleteAsync(int passengerId);
        Task<PassengerDto?> GetByIdAsync(int passengerId);
        Task<IEnumerable<PassengerDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<IEnumerable<PassengerDto>> GetByBookingIdAsync(int bookingId);
        Task<IEnumerable<PassengerDto>> GetByPassportNumberAsync(string passportNumber);
        Task<bool> ExistsAsync(int passengerId);
        Task<GuestBookingDetailsDto?> GetGuestBookingDetailsAsync(string bookingRef, string lastName);
        Task<bool> PassportNumberExistsAsync(string passportNumber, int excludePassengerId);
    }
}