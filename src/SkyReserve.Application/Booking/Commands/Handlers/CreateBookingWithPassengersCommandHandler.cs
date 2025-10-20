using MediatR;
using SkyReserve.Application.Booking.Commands.Models;
using SkyReserve.Application.Booking.DTOS;
using SkyReserve.Application.Booking.Queries.Models;
using SkyReserve.Application.DTOs.Payment.DTOs;
using SkyReserve.Application.Interfaces;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Booking.Commands.Handlers
{
    public class CreateBookingWithPassengersCommandHandler : IRequestHandler<CreateBookingWithPassengersCommand, BookingDto>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly IPriceRepository _priceRepository;
        private readonly IPassengerRepository _passengerRepository;
        private readonly IStripePayment _stripePaymentService;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMediator _mediator;

        public CreateBookingWithPassengersCommandHandler(
            IBookingRepository bookingRepository,
            IFlightRepository flightRepository,
            IPriceRepository priceRepository,
            IPassengerRepository passengerRepository,
            IStripePayment stripePaymentService,
            IPaymentRepository paymentRepository,
            IMediator mediator)
        {
            _bookingRepository = bookingRepository;
            _flightRepository = flightRepository;
            _priceRepository = priceRepository;
            _passengerRepository = passengerRepository;
            _stripePaymentService = stripePaymentService;
            _paymentRepository = paymentRepository;
            _mediator = mediator;
        }

        public async Task<BookingDto> Handle(CreateBookingWithPassengersCommand request, CancellationToken cancellationToken)
        {
            var flight = await _flightRepository.GetByIdAsync(request.FlightId);
            if (flight == null)
                throw new ArgumentException($"Flight with ID {request.FlightId} not found");

            if (request.Passengers == null || !request.Passengers.Any())
                throw new ArgumentException("At least one passenger is required");

            if (!await _flightRepository.HasAvailableSeatsAsync(request.FlightId, request.Passengers.Count))
                throw new InvalidOperationException($"Flight does not have {request.Passengers.Count} available seats");

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

            var passportNumbers = request.Passengers.Select(p => p.PassportNumber).ToList();
            if (passportNumbers.Count != passportNumbers.Distinct().Count())
                throw new ArgumentException("Duplicate passport numbers are not allowed in the same booking");

            var bookingDto = new CreateBookingDto
            {
                UserId = request.UserId,
                FlightId = request.FlightId,
                PriceId = price.Id,
                FareClass = request.FareClass,
                TotalAmount = calculatedTotal,
                BookingRef = await _bookingRepository.GenerateBookingRefAsync(),
                Status = "Pending",
                BookingDate = DateTime.UtcNow
            };

            var createdBooking = await _bookingRepository.CreateAsync(bookingDto);

            try
            {
                foreach (var passenger in request.Passengers)
                {
                    passenger.BookingId = createdBooking.BookingId;
                    await _passengerRepository.CreateAsync(passenger);
                }

                var seatUpdateSuccess = await _flightRepository.UpdateAvailableSeatsAsync(request.FlightId, -request.Passengers.Count);
                if (!seatUpdateSuccess)
                {
                    throw new InvalidOperationException("Failed to update available seats");
                }

                var paymentIntentResponse = await CreatePaymentIntentAsync(createdBooking, calculatedTotal);
                await CreatePendingPaymentAsync(createdBooking.BookingId, calculatedTotal, paymentIntentResponse, request.UserId, cancellationToken);

                return await _bookingRepository.GetByIdAsync(createdBooking.BookingId) ?? createdBooking;
            }
            catch
            {
                await _bookingRepository.DeleteAsync(createdBooking.BookingId);
                throw;
            }
        }

        private async Task<PaymentIntentResponse> CreatePaymentIntentAsync(BookingDto booking, decimal amount)
        {
            var amountInCents = (long)(amount * 100);

            var paymentRequest = new CreatePaymentIntentRequest
            {
                Amount = amountInCents,
                BookingId = booking.BookingId,
                Description = $"Flight booking payment for {booking.BookingRef}"
            };

            var response = await _stripePaymentService.CreatePaymentIntentForOrderAsync(paymentRequest);
            return response ?? throw new InvalidOperationException("Failed to create payment intent");
        }

        private async Task CreatePendingPaymentAsync(int bookingId, decimal amount, PaymentIntentResponse paymentIntent, string userId, CancellationToken cancellationToken)
        {
            var payment = new SkyReserve.Domain.Entities.Payment
            {
                BookingId = bookingId,
                Amount = amount,
                PaymentStatus = "Pending",
                PaymentMethod = "Credit Card",
                TransactionId = paymentIntent.PaymentIntentId,
                StripePaymentIntentId = paymentIntent.PaymentIntentId,
                PaymentDate = DateTime.UtcNow,
                UserId = userId ?? throw new ArgumentNullException(nameof(userId))
            };

            await _paymentRepository.AddAsync(payment, cancellationToken);
        }
    }
}