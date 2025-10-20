using FluentValidation;
using SkyReserve.Application.Booking.Commands.Models;
using SkyReserve.Application.Interfaces;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Booking.Commands.Validators
{
    public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
    {
        private readonly IFlightRepository _flightRepository;
        private readonly IPriceRepository _priceRepository;

        public CreateBookingCommandValidator(IFlightRepository flightRepository, IPriceRepository priceRepository)
        {
            _flightRepository = flightRepository;
            _priceRepository = priceRepository;

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required.")
                .MaximumLength(450)
                .WithMessage("User ID cannot exceed 450 characters.");

            RuleFor(x => x.FlightId)
                .GreaterThan(0)
                .WithMessage("Flight ID must be greater than 0.")
                .MustAsync(FlightMustExist)
                .WithMessage("Flight with ID '{PropertyValue}' does not exist.");

            RuleFor(x => x.FareClass)
                .NotEmpty()
                .WithMessage("Fare class is required.")
                .Must(BeValidFareClass)
                .WithMessage("Fare class must be one of: Economy, Business, First.");

            RuleFor(x => x)
                .MustAsync(FlightMustHaveAvailableSeats)
                .WithMessage("Flight does not have available seats for booking.")
                .MustAsync(ActivePriceMustExist)
                .WithMessage("No active pricing found for the selected flight and fare class.");
        }

        private async Task<bool> FlightMustExist(int flightId, CancellationToken cancellationToken)
        {
            return await _flightRepository.ExistsAsync(flightId);
        }

        private async Task<bool> FlightMustHaveAvailableSeats(CreateBookingCommand command, CancellationToken cancellationToken)
        {
            return await _flightRepository.HasAvailableSeatsAsync(command.FlightId, 1);
        }

        private async Task<bool> ActivePriceMustExist(CreateBookingCommand command, CancellationToken cancellationToken)
        {
            var price = await _priceRepository.GetActiveByFlightAndFareClassAsync(
                command.FlightId, command.FareClass, DateTime.UtcNow);
            return price != null;
        }

        private static bool BeValidFareClass(string fareClass)
        {
            var validFareClasses = new[] { "Economy", "Business", "First" };
            return validFareClasses.Contains(fareClass);
        }
    }
}