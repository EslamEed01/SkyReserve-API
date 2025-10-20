using FluentValidation;
using SkyReserve.Application.Flight.Queries.Models;

namespace SkyReserve.Application.Flight.Queries.Validators
{
    public class GetFlightByIdQueryValidator : AbstractValidator<GetFlightByIdQuery>
    {
        public GetFlightByIdQueryValidator()
        {
            RuleFor(x => x.FlightId)
                .GreaterThan(0)
                .WithMessage("Flight ID must be greater than 0.");
        }
    }
}