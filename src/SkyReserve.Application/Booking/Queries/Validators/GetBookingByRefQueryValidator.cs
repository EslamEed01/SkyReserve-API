using FluentValidation;
using SkyReserve.Application.Booking.Queries.Models;

namespace SkyReserve.Application.Booking.Queries.Validators
{
    public class GetBookingByRefQueryValidator : AbstractValidator<GetBookingByRefQuery>
    {
        public GetBookingByRefQueryValidator()
        {
            RuleFor(x => x.BookingRef)
                .NotEmpty()
                .WithMessage("Booking reference is required.")
                .Length(6, 20)
                .WithMessage("Booking reference must be between 6 and 20 characters.")
                .Matches(@"^[A-Z0-9]+$")
                .WithMessage("Booking reference must contain only uppercase letters and numbers.");
        }
    }
}