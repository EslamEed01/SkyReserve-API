using FluentValidation;
using SkyReserve.Application.Passenger.Queries.Models;

namespace SkyReserve.Application.Passenger.Queries.Validators
{
    public class GetAllPassengersQueryValidator : AbstractValidator<GetAllPassengersQuery>
    {
        public GetAllPassengersQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100.");
        }
    }
}