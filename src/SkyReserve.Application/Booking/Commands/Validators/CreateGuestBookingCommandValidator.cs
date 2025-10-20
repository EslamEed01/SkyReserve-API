using FluentValidation;
using SkyReserve.Application.Booking.Commands.Models;
using SkyReserve.Application.Booking.DTOS;

namespace SkyReserve.Application.Booking.Commands.Validators
{
    public class CreateGuestBookingCommandValidator : AbstractValidator<CreateGuestBookingCommand>
    {
        public CreateGuestBookingCommandValidator()
        {
            RuleFor(x => x.FlightId)
                .GreaterThan(0)
                .WithMessage("Flight ID must be greater than 0");

            RuleFor(x => x.Passengers)
                .NotEmpty()
                .WithMessage("At least one passenger is required")
                .Must(passengers => passengers.Count <= 10)
                .WithMessage("Maximum 10 passengers allowed per booking");

            RuleForEach(x => x.Passengers).SetValidator(new GuestPassengerDtoValidator());
        }
    }

    public class GuestPassengerDtoValidator : AbstractValidator<GuestPassengerDto>
    {
        public GuestPassengerDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("First name is required")
                .MaximumLength(100)
                .WithMessage("First name cannot exceed 100 characters");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last name is required")
                .MaximumLength(100)
                .WithMessage("Last name cannot exceed 100 characters");

            RuleFor(x => x.DateOfBirth)
                .NotEmpty()
                .WithMessage("Date of birth is required")
                .LessThan(DateTime.Now)
                .WithMessage("Date of birth must be in the past");

            RuleFor(x => x.PassportNumber)
                .NotEmpty()
                .WithMessage("Passport number is required")
                .MaximumLength(50)
                .WithMessage("Passport number cannot exceed 50 characters");

            RuleFor(x => x.Nationality)
                .NotEmpty()
                .WithMessage("Nationality is required")
                .MaximumLength(100)
                .WithMessage("Nationality cannot exceed 100 characters");
        }
    }
}