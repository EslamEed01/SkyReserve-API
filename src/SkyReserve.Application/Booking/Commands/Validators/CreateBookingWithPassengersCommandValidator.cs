using FluentValidation;
using SkyReserve.Application.Booking.Commands.Models;
using SkyReserve.Application.Interfaces;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Booking.Commands.Validators
{
    public class CreateBookingWithPassengersCommandValidator : AbstractValidator<CreateBookingWithPassengersCommand>
    {
        private readonly IFlightRepository _flightRepository;
        private readonly IPriceRepository _priceRepository;

        public CreateBookingWithPassengersCommandValidator(IFlightRepository flightRepository, IPriceRepository priceRepository)
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

            RuleFor(x => x.Passengers)
                .NotEmpty()
                .WithMessage("At least one passenger is required.")
                .Must(x => x.Count <= 10)
                .WithMessage("Cannot book for more than 10 passengers at once.");

            RuleFor(x => x)
                .MustAsync(FlightMustHaveAvailableSeats)
                .WithMessage("Flight does not have enough available seats for all passengers.")
                .MustAsync(ActivePriceMustExist)
                .WithMessage("No active pricing found for the selected flight and fare class.");

            RuleForEach(x => x.Passengers).ChildRules(passenger =>
            {
                passenger.RuleFor(p => p.FirstName)
                    .NotEmpty()
                    .WithMessage("Passenger first name is required.")
                    .MaximumLength(100)
                    .WithMessage("First name cannot exceed 100 characters.");

                passenger.RuleFor(p => p.LastName)
                    .NotEmpty()
                    .WithMessage("Passenger last name is required.")
                    .MaximumLength(100)
                    .WithMessage("Last name cannot exceed 100 characters.");

                passenger.RuleFor(p => p.PassportNumber)
                    .NotEmpty()
                    .WithMessage("Passport number is required.")
                    .MaximumLength(50)
                    .WithMessage("Passport number cannot exceed 50 characters.");
            });
        }

        private async Task<bool> FlightMustExist(int flightId, CancellationToken cancellationToken)
        {
            return await _flightRepository.ExistsAsync(flightId);
        }

        private async Task<bool> FlightMustHaveAvailableSeats(CreateBookingWithPassengersCommand command, CancellationToken cancellationToken)
        {
            return await _flightRepository.HasAvailableSeatsAsync(command.FlightId, command.Passengers.Count);
        }

        private async Task<bool> ActivePriceMustExist(CreateBookingWithPassengersCommand command, CancellationToken cancellationToken)
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