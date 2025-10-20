using FluentValidation;
using SkyReserve.Application.Flight.Queries.Models;

namespace SkyReserve.Application.Flight.Queries.Validators
{
    public class GetAllFlightsQueryValidator : AbstractValidator<GetAllFlightsQuery>
    {
        public GetAllFlightsQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100.");

            RuleFor(x => x.Status)
                .Must(BeValidStatus)
                .WithMessage("Status must be one of: Scheduled, Delayed, Cancelled, Completed, Boarding, In-Flight.")
                .When(x => !string.IsNullOrEmpty(x.Status));

            RuleFor(x => x.DepartureAirportId)
                .GreaterThan(0)
                .WithMessage("Departure airport ID must be greater than 0.")
                .When(x => x.DepartureAirportId.HasValue);

            RuleFor(x => x.ArrivalAirportId)
                .GreaterThan(0)
                .WithMessage("Arrival airport ID must be greater than 0.")
                .When(x => x.ArrivalAirportId.HasValue);

            RuleFor(x => x)
                .Must(x => x.DepartureAirportId != x.ArrivalAirportId)
                .WithMessage("Departure and arrival airports cannot be the same.")
                .When(x => x.DepartureAirportId.HasValue && x.ArrivalAirportId.HasValue);

            RuleFor(x => x.DepartureDate)
                .GreaterThanOrEqualTo(DateTime.Today.AddDays(-1))
                .WithMessage("Departure date cannot be more than 1 day in the past.")
                .LessThanOrEqualTo(DateTime.Today.AddYears(2))
                .WithMessage("Departure date cannot be more than 2 years in the future.")
                .When(x => x.DepartureDate.HasValue);
        }

        private static bool BeValidStatus(string? status)
        {
            if (string.IsNullOrEmpty(status))
                return true;

            var validStatuses = new[] { "Scheduled", "Delayed", "Cancelled", "Completed", "Boarding", "In-Flight" };
            return validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
        }
    }
}