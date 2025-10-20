using FluentValidation;
using SkyReserve.Application.Booking.Queries.Models;

namespace SkyReserve.Application.Booking.Queries.Validators
{
    public class GetGuestBookingQueryValidator : AbstractValidator<GetGuestBookingQuery>
    {
        public GetGuestBookingQueryValidator()
        {
            RuleFor(x => x.BookingRef)
                .NotEmpty()
                .WithMessage("Booking reference is required")
                .Length(6, 20)
                .WithMessage("Booking reference must be between 6 and 20 characters");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last name is required")
                .MaximumLength(100)
                .WithMessage("Last name cannot exceed 100 characters");
        }
    }
}