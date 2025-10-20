using FluentValidation;
using SkyReserve.Application.Booking.Queries.Models;

namespace SkyReserve.Application.Booking.Queries.Validators
{
    public class GetUserBookingsQueryValidator : AbstractValidator<GetUserBookingsQuery>
    {
        public GetUserBookingsQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required.")
                .MaximumLength(450)
                .WithMessage("User ID cannot exceed 450 characters.");
        }
    }
}