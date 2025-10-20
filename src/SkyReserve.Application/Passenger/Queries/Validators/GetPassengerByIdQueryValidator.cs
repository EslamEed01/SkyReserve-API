using FluentValidation;
using SkyReserve.Application.Passenger.Queries.Models;

namespace SkyReserve.Application.Passenger.Queries.Validators
{
    public class GetPassengerByIdQueryValidator : AbstractValidator<GetPassengerByIdQuery>
    {
        public GetPassengerByIdQueryValidator()
        {
            RuleFor(x => x.PassengerId)
                .GreaterThan(0)
                .WithMessage("Passenger ID must be greater than 0.");
        }
    }
}