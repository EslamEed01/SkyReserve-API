using FluentValidation;
using SkyReserve.Application.Passenger.Queries.Models;

namespace SkyReserve.Application.Passenger.Queries.Validators
{
    public class GetPassengersByPassportNumberQueryValidator : AbstractValidator<GetPassengersByPassportNumberQuery>
    {
        public GetPassengersByPassportNumberQueryValidator()
        {
            RuleFor(x => x.PassportNumber)
                .NotEmpty()
                .WithMessage("Passport number is required.")
                .Length(6, 15)
                .WithMessage("Passport number must be between 6 and 15 characters.")
                .Matches(@"^[A-Z0-9]+$")
                .WithMessage("Passport number can only contain uppercase letters and numbers.");
        }
    }
}