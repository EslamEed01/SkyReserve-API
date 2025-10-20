using MediatR;
using SkyReserve.Application.Booking.Commands.Models;
using SkyReserve.Application.Booking.DTOS;
using SkyReserve.Application.Booking.Queries.Models;
using SkyReserve.Application.Interfaces;
using SkyReserve.Application.Passenger.DTOS;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Booking.Commands.Handlers
{
    public class CreateGuestBookingCommandHandler : IRequestHandler<CreateGuestBookingCommand, BookingDto>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly IPriceRepository _priceRepository;
        private readonly IPassengerRepository _passengerRepository;
        private readonly IMediator _mediator;

        public CreateGuestBookingCommandHandler(
            IBookingRepository bookingRepository,
            IFlightRepository flightRepository,
            IPriceRepository priceRepository,
            IPassengerRepository passengerRepository,
            IMediator mediator)
        {
            _bookingRepository = bookingRepository;
            _flightRepository = flightRepository;
            _priceRepository = priceRepository;
            _passengerRepository = passengerRepository;
            _mediator = mediator;
        }

        public async Task<BookingDto> Handle(CreateGuestBookingCommand request, CancellationToken cancellationToken)
        {
            var flight = await _flightRepository.GetByIdAsync(request.FlightId);
            if (flight == null)
                throw new ArgumentException($"Flight with ID {request.FlightId} not found");

            var availableSeats = await _flightRepository.GetAvailableSeatsAsync(request.FlightId);
            if (availableSeats < request.Passengers.Count)
                throw new InvalidOperationException("Not enough seats available for this flight");

            var currentDate = DateTime.UtcNow;
            var price = await _priceRepository.GetActiveByFlightAndFareClassAsync(
                request.FlightId, request.FareClass, currentDate);

            if (price == null)
            {
                throw new InvalidOperationException($"No active pricing found for flight {request.FlightId} with fare class {request.FareClass}");
            }

            var calculatedTotal = await _mediator.Send(new CalculateBookingTotalQuery
            {
                FlightId = request.FlightId,
                PassengerCount = request.Passengers.Count,
                FareClass = request.FareClass
            }, cancellationToken);

            var bookingRef = GenerateBookingReference();

            var createBookingDto = new CreateBookingDto
            {
                BookingRef = bookingRef,
                UserId = null,
                FlightId = request.FlightId,
                PriceId = price.Id,
                FareClass = request.FareClass,
                BookingDate = DateTime.UtcNow,
                Status = "Pending",
                TotalAmount = calculatedTotal
            };

            var savedBooking = await _bookingRepository.CreateAsync(createBookingDto);

            foreach (var passengerDto in request.Passengers)
            {
                var createPassengerDto = new CreatePassengerDto
                {
                    BookingId = savedBooking.BookingId,
                    UserId = null,
                    FirstName = passengerDto.FirstName,
                    LastName = passengerDto.LastName,
                    DateOfBirth = passengerDto.DateOfBirth,
                    PassportNumber = passengerDto.PassportNumber,
                    Nationality = passengerDto.Nationality
                };

                await _passengerRepository.CreateAsync(createPassengerDto);
            }

            await _flightRepository.UpdateAvailableSeatsAsync(request.FlightId, -request.Passengers.Count);

            return await _bookingRepository.GetByIdAsync(savedBooking.BookingId) ?? savedBooking;
        }

        private string GenerateBookingReference()
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}