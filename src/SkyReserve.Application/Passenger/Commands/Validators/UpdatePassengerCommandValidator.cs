using FluentValidation;
using SkyReserve.Application.Passenger.Commands.Models;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Passenger.Commands.Validators
{
    public class UpdatePassengerCommandValidator : AbstractValidator<UpdatePassengerCommand>
    {
        private readonly IPassengerRepository _passengerRepository;

        public UpdatePassengerCommandValidator(IPassengerRepository passengerRepository)
        {
            _passengerRepository = passengerRepository;

            RuleFor(x => x.PassengerId)
                .GreaterThan(0)
                .WithMessage("Passenger ID must be greater than 0.")
                .MustAsync(PassengerExists)
                .WithMessage("Passenger with this ID does not exist.");

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("First name is required.")
                .Length(2, 50)
                .WithMessage("First name must be between 2 and 50 characters.")
                .Matches(@"^[a-zA-Z\s'-]+$")
                .WithMessage("First name can only contain letters, spaces, hyphens, and apostrophes.");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last name is required.")
                .Length(2, 50)
                .WithMessage("Last name must be between 2 and 50 characters.")
                .Matches(@"^[a-zA-Z\s'-]+$")
                .WithMessage("Last name can only contain letters, spaces, hyphens, and apostrophes.");

            RuleFor(x => x.DateOfBirth)
                .NotEmpty()
                .WithMessage("Date of birth is required.")
                .LessThan(DateTime.Today)
                .WithMessage("Date of birth must be in the past.")
                .GreaterThan(DateTime.Today.AddYears(-120))
                .WithMessage("Date of birth cannot be more than 120 years ago.")
                .LessThan(DateTime.Today.AddYears(-1))
                .WithMessage("Passenger must be at least 1 year old.");

            RuleFor(x => x.PassportNumber)
                .NotEmpty()
                .WithMessage("Passport number is required.")
                .Length(6, 15)
                .WithMessage("Passport number must be between 6 and 15 characters.")
                .Matches(@"^[A-Z0-9]+$")
                .WithMessage("Passport number can only contain uppercase letters and numbers.")
                .MustAsync(BeUniquePassportNumberForUpdate)
                .WithMessage("A passenger with this passport number already exists.");

            RuleFor(x => x.Nationality)
                .NotEmpty()
                .WithMessage("Nationality is required.")
                .Length(2, 50)
                .WithMessage("Nationality must be between 2 and 50 characters.")
                .Matches(@"^[a-zA-Z\s]+$")
                .WithMessage("Nationality can only contain letters and spaces.");
        }

        private async Task<bool> PassengerExists(int passengerId, CancellationToken cancellationToken)
        {
            return await _passengerRepository.ExistsAsync(passengerId);
        }

        private async Task<bool> BeUniquePassportNumberForUpdate(UpdatePassengerCommand command, string passportNumber, CancellationToken cancellationToken)
        {
            return !await _passengerRepository.PassportNumberExistsAsync(passportNumber, command.PassengerId);
        }
    }
}