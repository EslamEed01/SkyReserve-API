using SkyReserve.Application.Booking.DTOS;

namespace SkyReserve.Application.Repository
{
    public interface IBookingRepository
    {
        Task<BookingDto> CreateAsync(CreateBookingDto booking);
        Task<BookingDto> UpdateAsync(UpdateBookingDto booking);
        Task<bool> DeleteAsync(int bookingId);
        Task<BookingDto?> GetByIdAsync(int bookingId);
        Task<BookingDto?> GetByBookingRefAsync(string bookingRef);
        Task<IEnumerable<BookingDto>> GetByUserIdAsync(string userId);
        Task<IEnumerable<BookingDto>> GetByFlightIdAsync(int flightId);
        Task<bool> ExistsAsync(int bookingId);
        Task<bool> BookingRefExistsAsync(string bookingRef);
        Task<string> GenerateBookingRefAsync();
        Task<bool> CancelBookingWithRefundAsync(int bookingId, string cancellationReason);
        Task<bool> UpdateBookingStatusWithTransactionAsync(int bookingId, string status);
    }
}