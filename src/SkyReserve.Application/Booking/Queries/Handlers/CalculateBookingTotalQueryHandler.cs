using MediatR;
using SkyReserve.Application.Booking.Queries.Models;
using SkyReserve.Application.Interfaces;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Booking.Queries.Handlers
{
    public class CalculateBookingTotalQueryHandler : IRequestHandler<CalculateBookingTotalQuery, decimal>
    {
        private readonly IFlightRepository _flightRepository;
        private readonly IPriceRepository _priceRepository;

        public CalculateBookingTotalQueryHandler(
            IFlightRepository flightRepository,
            IPriceRepository priceRepository)
        {
            _flightRepository = flightRepository;
            _priceRepository = priceRepository;
        }

        public async Task<decimal> Handle(CalculateBookingTotalQuery request, CancellationToken cancellationToken)
        {
            var flight = await _flightRepository.GetByIdAsync(request.FlightId);
            if (flight == null)
                throw new ArgumentException($"Flight with ID {request.FlightId} not found");

            var fareClass = request.FareClass ?? "Economy";
            var currentDate = DateTime.UtcNow;

            var price = await _priceRepository.GetActiveByFlightAndFareClassAsync(
                request.FlightId, fareClass, currentDate);

            if (price == null)
            {
                var activePrices = await _priceRepository.GetActivePricesAsync(request.FlightId, currentDate);
                price = activePrices.FirstOrDefault();
            }

            if (price == null)
            {
                throw new InvalidOperationException($"No active pricing found for flight {request.FlightId}");
            }

            decimal basePrice = price.BasePrice;
            decimal taxes = basePrice * 0.1m; // 10% tax
            decimal serviceFee = 25.00m; // Fixed service fee

            return (basePrice + taxes) * request.PassengerCount + serviceFee;
        }
    }
}