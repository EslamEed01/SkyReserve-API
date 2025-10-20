using FluentValidation;
using SkyReserve.Application.Passenger.Queries.Models;

namespace SkyReserve.Application.Passenger.Queries.Validators
{
    public class GetPassengersByBookingIdQueryValidator : AbstractValidator<GetPassengersByBookingIdQuery>
    {
        public GetPassengersByBookingIdQueryValidator()
        {
            RuleFor(x => x.BookingId)
                .GreaterThan(0)
                .WithMessage("Booking ID must be greater than 0.");
        }
    }
}