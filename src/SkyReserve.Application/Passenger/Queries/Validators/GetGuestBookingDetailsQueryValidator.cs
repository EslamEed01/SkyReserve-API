using FluentValidation;
using SkyReserve.Application.Passenger.Queries.Models;

namespace SkyReserve.Application.Passenger.Queries.Validators
{
    public class GetGuestBookingDetailsQueryValidator : AbstractValidator<GetGuestBookingDetailsQuery>
    {
        public GetGuestBookingDetailsQueryValidator()
        {
            RuleFor(x => x.BookingRef)
                .NotEmpty()
                .WithMessage("Booking reference is required")
                .Length(3, 20)
                .WithMessage("Booking reference must be between 3 and 20 characters");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last name is required")
                .Length(1, 100)
                .WithMessage("Last name must be between 1 and 100 characters");
        }
    }
}