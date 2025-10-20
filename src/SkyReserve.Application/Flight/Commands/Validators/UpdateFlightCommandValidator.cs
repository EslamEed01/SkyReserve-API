using FluentValidation;
using SkyReserve.Application.Flight.Commands.Models;
using SkyReserve.Application.Interfaces;

namespace SkyReserve.Application.Flight.Commands.Validators
{
    public class UpdateFlightCommandValidator : AbstractValidator<UpdateFlightCommand>
    {
        private readonly IFlightRepository _flightRepository;

        public UpdateFlightCommandValidator(IFlightRepository flightRepository)
        {
            _flightRepository = flightRepository;

            RuleFor(x => x.FlightId)
                .GreaterThan(0).WithMessage("Flight ID must be greater than 0.")
                .MustAsync(FlightMustExist).WithMessage("Flight with ID '{PropertyValue}' does not exist.");

            RuleFor(x => x.FlightNumber)
                .Length(3, 20).WithMessage("Flight number must be between 3 and 20 characters.")
                .Matches(@"^[A-Z]{2,3}[0-9]{1,4}$").WithMessage("Flight number must follow the format: 2-3 letters followed by 1-4 digits (e.g., AA123, BAW456).")
                .MustAsync(BeUniqueFlightNumber).WithMessage("Flight number '{PropertyValue}' already exists.")
                .When(x => !string.IsNullOrEmpty(x.FlightNumber));

            RuleFor(x => x.DepartureAirportId)
                .GreaterThan(0).WithMessage("Departure airport must be selected.")
                .When(x => x.DepartureAirportId.HasValue);

            RuleFor(x => x.ArrivalAirportId)
                .GreaterThan(0).WithMessage("Arrival airport must be selected.")
                .When(x => x.ArrivalAirportId.HasValue);

            RuleFor(x => x)
                .Must(x => x.DepartureAirportId != x.ArrivalAirportId)
                .WithMessage("Departure and arrival airports cannot be the same.")
                .When(x => x.DepartureAirportId.HasValue && x.ArrivalAirportId.HasValue);

            RuleFor(x => x.DepartureTime)
                .GreaterThan(DateTime.UtcNow.AddMinutes(30)).WithMessage("Departure time must be at least 30 minutes from now.")
                .When(x => x.DepartureTime.HasValue);

            RuleFor(x => x.ArrivalTime)
                .GreaterThan(x => x.DepartureTime).WithMessage("Arrival time must be after departure time.")
                .When(x => x.ArrivalTime.HasValue && x.DepartureTime.HasValue);

            RuleFor(x => x)
                .Must(x => !x.DepartureTime.HasValue || !x.ArrivalTime.HasValue ||
                          x.ArrivalTime > x.DepartureTime.Value.AddMinutes(30))
                .WithMessage("Flight duration must be at least 30 minutes.")
                .When(x => x.DepartureTime.HasValue && x.ArrivalTime.HasValue);

            RuleFor(x => x.AircraftModel)
                .Length(2, 100).WithMessage("Aircraft model must be between 2 and 100 characters.")
                .Matches(@"^[A-Za-z0-9\s\-]+$").WithMessage("Aircraft model can only contain letters, numbers, spaces, and hyphens.")
                .When(x => !string.IsNullOrEmpty(x.AircraftModel));

            RuleFor(x => x.Status)
                .Must(BeValidStatus).WithMessage("Status must be one of: Scheduled, Delayed, Cancelled, Completed, Boarding, In-Flight.")
                .When(x => !string.IsNullOrEmpty(x.Status));
        }

        private async Task<bool> FlightMustExist(int flightId, CancellationToken cancellationToken)
        {
            return await _flightRepository.ExistsAsync(flightId);
        }

        private async Task<bool> BeUniqueFlightNumber(UpdateFlightCommand command, string flightNumber, CancellationToken cancellationToken)
        {
            return !await _flightRepository.FlightNumberExistsAsync(flightNumber, command.FlightId);
        }

        private static bool BeValidStatus(string status)
        {
            var validStatuses = new[] { "Scheduled", "Delayed", "Cancelled", "Completed", "Boarding", "In-Flight" };
            return validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
        }
    }
}
