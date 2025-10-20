using FluentValidation;
using SkyReserve.Application.Booking.Queries.Models;

namespace SkyReserve.Application.Booking.Queries.Validators
{
    public class GetBookingByIdQueryValidator : AbstractValidator<GetBookingByIdQuery>
    {
        public GetBookingByIdQueryValidator()
        {
            RuleFor(x => x.BookingId)
                .GreaterThan(0)
                .WithMessage("Booking ID must be greater than 0.");
        }
    }
}