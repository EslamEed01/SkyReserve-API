using FluentValidation;
using SkyReserve.Application.Flight.Commands.Models;
using SkyReserve.Application.Interfaces;

namespace SkyReserve.Application.Flight.Commands.Validators
{
    public class CreateFlightCommandValidator : AbstractValidator<CreateFlightCommand>
    {
        private readonly IFlightRepository _flightRepository;

        public CreateFlightCommandValidator(IFlightRepository flightRepository)
        {
            _flightRepository = flightRepository;

            RuleFor(x => x.FlightNumber)
                .NotEmpty().WithMessage("Flight number is required.")
                .Length(3, 20).WithMessage("Flight number must be between 3 and 20 characters.")
                .Matches(@"^[A-Z]{2,3}[0-9]{1,4}$").WithMessage("Flight number must follow the format: 2-3 letters followed by 1-4 digits (e.g., AA123, BAW456).")
                .MustAsync(BeUniqueFlightNumber).WithMessage("Flight number '{PropertyValue}' already exists.");

            RuleFor(x => x.DepartureAirportId)
                .GreaterThan(0).WithMessage("Departure airport must be selected.");

            RuleFor(x => x.ArrivalAirportId)
                .GreaterThan(0).WithMessage("Arrival airport must be selected.")
                .NotEqual(x => x.DepartureAirportId).WithMessage("Departure and arrival airports cannot be the same.");

            RuleFor(x => x.DepartureTime)
                .NotEmpty().WithMessage("Departure time is required.")
                .GreaterThan(DateTime.UtcNow.AddMinutes(30)).WithMessage("Departure time must be at least 30 minutes from now.");

            RuleFor(x => x.ArrivalTime)
                .NotEmpty().WithMessage("Arrival time is required.")
                .GreaterThan(x => x.DepartureTime).WithMessage("Arrival time must be after departure time.")
                .GreaterThan(x => x.DepartureTime.AddMinutes(30)).WithMessage("Flight duration must be at least 30 minutes.");

            RuleFor(x => x.AircraftModel)
                .NotEmpty().WithMessage("Aircraft model is required.")
                .Length(2, 100).WithMessage("Aircraft model must be between 2 and 100 characters.")
                .Matches(@"^[A-Za-z0-9\s\-]+$").WithMessage("Aircraft model can only contain letters, numbers, spaces, and hyphens.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(BeValidStatus).WithMessage("Status must be one of: Scheduled, Delayed, Cancelled, Completed, Boarding, In-Flight.");
        }

        private async Task<bool> BeUniqueFlightNumber(string flightNumber, CancellationToken cancellationToken)
        {
            return !await _flightRepository.FlightNumberExistsAsync(flightNumber);
        }

        private static bool BeValidStatus(string status)
        {
            var validStatuses = new[] { "Scheduled", "Delayed", "Cancelled", "Completed", "Boarding", "In-Flight" };
            return validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
        }
    }
}
