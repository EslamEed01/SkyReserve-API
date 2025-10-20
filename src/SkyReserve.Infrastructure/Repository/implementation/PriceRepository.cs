using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SkyReserve.Application.DTOs.Price.DTOs;
using SkyReserve.Application.Repository;
using SkyReserve.Domain.Entities;
using SkyReserve.Infrastructure.Persistence;

namespace SkyReserve.Infrastructure.Repository.implementation
{
    public class PriceRepository : IPriceRepository
    {
        private readonly SkyReserveDbContext _context;
        private readonly IMapper _mapper;

        public PriceRepository(SkyReserveDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PriceDto> CreateAsync(CreatePriceDto price)
        {
            var priceEntity = _mapper.Map<Price>(price);
            priceEntity.CreatedAt = DateTime.UtcNow;
            priceEntity.UpdatedAt = DateTime.UtcNow;

            var flightExists = await _context.Flights.AnyAsync(f => f.FlightId == price.FlightId);
            if (!flightExists)
                throw new ArgumentException($"Flight with ID {price.FlightId} not found");

            if (price.ValidFrom >= price.ValidTo)
                throw new ArgumentException("ValidFrom must be before ValidTo");

            _context.Prices.Add(priceEntity);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(priceEntity.Id) ?? new PriceDto();
        }

        public async Task<PriceDto?> GetByIdAsync(int id)
        {
            var price = await _context.Prices
                .Include(p => p.Flight)
                .FirstOrDefaultAsync(p => p.Id == id);

            return price == null ? null : _mapper.Map<PriceDto>(price);
        }

        public async Task<IEnumerable<PriceDto>> GetByFlightIdAsync(int flightId)
        {
            var prices = await _context.Prices
                .Include(p => p.Flight)
                .Where(p => p.FlightId == flightId)
                .OrderBy(p => p.FareClass)
                .ThenBy(p => p.ValidFrom)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PriceDto>>(prices);
        }

        public async Task<PriceDto?> GetActiveByFlightAndFareClassAsync(int flightId, string fareClass, DateTime bookingDate)
        {
            var price = await _context.Prices
                .Include(p => p.Flight)
                .Where(p => p.FlightId == flightId
                         && p.FareClass == fareClass
                         && p.ValidFrom <= bookingDate
                         && p.ValidTo > bookingDate)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            return price == null ? null : _mapper.Map<PriceDto>(price);
        }

        public async Task<IEnumerable<PriceDto>> GetActivePricesAsync(int flightId, DateTime bookingDate)
        {
            var prices = await _context.Prices
                .Include(p => p.Flight)
                .Where(p => p.FlightId == flightId
                         && p.ValidFrom <= bookingDate
                         && p.ValidTo > bookingDate)
                .OrderBy(p => p.FareClass)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PriceDto>>(prices);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var price = await _context.Prices
                .FirstOrDefaultAsync(p => p.Id == id);

            if (price == null)
                return false;

            var isUsedInBookings = await _context.Bookings
                .AnyAsync(b => b.PriceId == id);

            if (isUsedInBookings)
                throw new InvalidOperationException("Cannot delete price that is referenced by existing bookings");

            _context.Prices.Remove(price);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Prices.AnyAsync(p => p.Id == id);
        }
    }
}