using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SkyReserve.Application.Booking.DTOS;
using SkyReserve.Application.Repository;
using SkyReserve.Domain.Entities;
using SkyReserve.Infrastructure.Persistence;

namespace SkyReserve.Infrastructure.Repository.implementation
{
    public class BookingRepository : IBookingRepository
    {
        private readonly SkyReserveDbContext _context;
        private readonly IMapper _mapper;

        public BookingRepository(SkyReserveDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<BookingDto> CreateAsync(CreateBookingDto booking)
        {
            if (string.IsNullOrEmpty(booking.UserId))
            {
                booking.UserId = null;
            }

            var bookingEntity = _mapper.Map<Booking>(booking);
            bookingEntity.BookingRef = await GenerateBookingRefAsync();
            bookingEntity.CreatedAt = DateTime.UtcNow;
            bookingEntity.UpdatedAt = DateTime.UtcNow;

            _context.Bookings.Add(bookingEntity);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(bookingEntity.BookingId) ?? new BookingDto();
        }

        public async Task<BookingDto> UpdateAsync(UpdateBookingDto booking)
        {
            var existingBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == booking.BookingId);

            if (existingBooking == null)
                throw new KeyNotFoundException($"Booking with ID {booking.BookingId} not found");

            _mapper.Map(booking, existingBooking);
            existingBooking.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(existingBooking.BookingId) ?? new BookingDto();
        }

        public async Task<bool> DeleteAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                return false;

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<BookingDto?> GetByIdAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Flight)
                    .ThenInclude(f => f.DepartureAirport)
                .Include(b => b.Flight)
                    .ThenInclude(f => f.ArrivalAirport)
                .Include(b => b.Payment)
                .Include(b => b.Passengers)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            return booking == null ? null : _mapper.Map<BookingDto>(booking);
        }

        public async Task<BookingDto?> GetByBookingRefAsync(string bookingRef)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Flight)
                    .ThenInclude(f => f.DepartureAirport)
                .Include(b => b.Flight)
                    .ThenInclude(f => f.ArrivalAirport)
                .Include(b => b.Payment)
                .Include(b => b.Passengers)
                .FirstOrDefaultAsync(b => b.BookingRef == bookingRef);

            return booking == null ? null : _mapper.Map<BookingDto>(booking);
        }

    
        public async Task<IEnumerable<BookingDto>> GetByUserIdAsync(string userId)
        {
            var bookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Flight)
                    .ThenInclude(f => f.DepartureAirport)
                .Include(b => b.Flight)
                    .ThenInclude(f => f.ArrivalAirport)
                .Include(b => b.Payment)
                .Where(b => b.UserId == userId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<BookingDto>>(bookings);
        }

        public async Task<IEnumerable<BookingDto>> GetByFlightIdAsync(int flightId)
        {
            var bookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Flight)
                    .ThenInclude(f => f.DepartureAirport)
                .Include(b => b.Flight)
                    .ThenInclude(f => f.ArrivalAirport)
                .Include(b => b.Payment)
                .Include(b => b.Passengers)
                .Where(b => b.FlightId == flightId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<BookingDto>>(bookings);
        }


        public async Task<bool> ExistsAsync(int bookingId)
        {
            return await _context.Bookings.AnyAsync(b => b.BookingId == bookingId);
        }

        public async Task<bool> BookingRefExistsAsync(string bookingRef)
        {
            return await _context.Bookings.AnyAsync(b => b.BookingRef == bookingRef);
        }

        public async Task<string> GenerateBookingRefAsync()
        {
            string bookingRef;
            do
            {
                bookingRef = GenerateRandomString(6);
            }
            while (await BookingRefExistsAsync(bookingRef));

            return bookingRef;
        }

        public async Task<bool> CancelBookingWithRefundAsync(int bookingId, string cancellationReason)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.Payment)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null)
                    return false;

                booking.Status = "Cancelled";
                booking.UpdatedAt = DateTime.UtcNow;

                if (booking.Payment != null)
                {
                    booking.Payment.PaymentStatus = "Refunded";
                    booking.Payment.TransactionId += $" | Cancelled: {cancellationReason}";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateBookingStatusWithTransactionAsync(int bookingId, string status)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var booking = await _context.Bookings
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null)
                    return false;

                booking.Status = status;
                booking.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
      
        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}