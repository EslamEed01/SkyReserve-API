using FluentValidation;
using SkyReserve.Application.Flight.Queries.Models;

namespace SkyReserve.Application.Flight.Queries.Validators
{
    public class GetFlightsByRouteQueryValidator : AbstractValidator<GetFlightsByRouteQuery>
    {
        public GetFlightsByRouteQueryValidator()
        {
            RuleFor(x => x.DepartureAirportId)
                .GreaterThan(0)
                .WithMessage("Departure airport ID must be greater than 0.");

            RuleFor(x => x.ArrivalAirportId)
                .GreaterThan(0)
                .WithMessage("Arrival airport ID must be greater than 0.")
                .NotEqual(x => x.DepartureAirportId)
                .WithMessage("Departure and arrival airports cannot be the same.");

            RuleFor(x => x.DepartureDate)
                .NotEmpty()
                .WithMessage("Departure date is required.")
                .GreaterThanOrEqualTo(DateTime.Today.AddDays(-1))
                .WithMessage("Departure date cannot be more than 1 day in the past.")
                .LessThanOrEqualTo(DateTime.Today.AddYears(2))
                .WithMessage("Departure date cannot be more than 2 years in the future.");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100.");
        }
    }
}