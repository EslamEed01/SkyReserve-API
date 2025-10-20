using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SkyReserve.Application.Passenger.DTOS;
using SkyReserve.Application.Repository;
using SkyReserve.Domain.Entities;
using SkyReserve.Infrastructure.Persistence;

namespace SkyReserve.Infrastructure.Repository.implementation
{
    public class PassengerRepository : IPassengerRepository
    {
        private readonly SkyReserveDbContext _context;
        private readonly IMapper _mapper;

        public PassengerRepository(SkyReserveDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

     
        public async Task<bool> DeleteAsync(int passengerId)
        {
            var passenger = await _context.Passengers
                .FirstOrDefaultAsync(p => p.PassengerId == passengerId);

            if (passenger == null)
                return false;

            _context.Passengers.Remove(passenger);
            await _context.SaveChangesAsync();
            return true;
        }

    
        public async Task<IEnumerable<PassengerDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            var passengers = await _context.Passengers
                .Include(p => p.Booking)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PassengerDto>>(passengers);
        }

        public async Task<IEnumerable<PassengerDto>> GetByBookingIdAsync(int bookingId)
        {
            var passengers = await _context.Passengers
                .Include(p => p.Booking)
                .Where(p => p.BookingId == bookingId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PassengerDto>>(passengers);
        }

      
        public async Task<bool> ExistsAsync(int passengerId)
        {
            return await _context.Passengers
                .AnyAsync(p => p.PassengerId == passengerId);
        }


        public async Task<GuestBookingDetailsDto?> GetGuestBookingDetailsAsync(string bookingRef, string lastName)
        {
            var booking = await _context.Bookings
                .Include(b => b.Flight)
                    .ThenInclude(f => f.DepartureAirport)
                .Include(b => b.Flight)
                    .ThenInclude(f => f.ArrivalAirport)
                .Include(b => b.Passengers.Where(p => p.LastName.ToLower() == lastName.ToLower()))
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.BookingRef == bookingRef);

            if (booking == null || !booking.Passengers.Any())
            {
                return null;
            }

            return _mapper.Map<GuestBookingDetailsDto>(booking);
        }

        public async Task<PassengerDto> CreateAsync(CreatePassengerDto passenger)
        {
            var passengerEntity = _mapper.Map<Passenger>(passenger);
            

            _context.Passengers.Add(passengerEntity);
            await _context.SaveChangesAsync();

            return _mapper.Map<PassengerDto>(passengerEntity);
        }

        public async Task<bool> PassportNumberExistsAsync(string passportNumber, int excludePassengerId)
        {
            return await _context.Passengers
                .AnyAsync(p => p.PassportNumber == passportNumber && p.PassengerId != excludePassengerId);
        }

        public async Task<PassengerDto> UpdateAsync(UpdatePassengerDto passenger)
        {
            var existingPassenger = await _context.Passengers
                .FirstOrDefaultAsync(p => p.PassengerId == passenger.PassengerId);

            if (existingPassenger == null)
                throw new KeyNotFoundException($"Passenger with ID {passenger.PassengerId} not found");

            _mapper.Map(passenger, existingPassenger);
            

            await _context.SaveChangesAsync();

            var updatedPassenger = await _context.Passengers
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.PassengerId == passenger.PassengerId);

            return _mapper.Map<PassengerDto>(updatedPassenger);
        }

        public async Task<PassengerDto?> GetByIdAsync(int passengerId)
        {
            var passenger = await _context.Passengers
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Flight)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(p => p.PassengerId == passengerId);

            return passenger == null ? null : _mapper.Map<PassengerDto>(passenger);
        }

        public async Task<IEnumerable<PassengerDto>> GetByPassportNumberAsync(string passportNumber)
        {
            var passengers = await _context.Passengers
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Flight)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.User)
                .Where(p => p.PassportNumber == passportNumber)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PassengerDto>>(passengers);
        }

      
    }
}