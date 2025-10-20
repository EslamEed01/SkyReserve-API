using FluentValidation;
using SkyReserve.Application.Booking.Commands.Models;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Booking.Commands.Validators
{
    public class CancelBookingCommandValidator : AbstractValidator<CancelBookingCommand>
    {
        private readonly IBookingRepository _bookingRepository;

        public CancelBookingCommandValidator(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;

            RuleFor(x => x.BookingId)
                .GreaterThan(0)
                .WithMessage("Booking ID must be greater than 0.")
                .MustAsync(BookingMustExist)
                .WithMessage("Booking with ID '{PropertyValue}' does not exist.");

            RuleFor(x => x.CancellationReason)
                .NotEmpty()
                .WithMessage("Cancellation reason is required.")
                .MaximumLength(500)
                .WithMessage("Cancellation reason cannot exceed 500 characters.");

            RuleFor(x => x)
                .MustAsync(BookingMustBeCancellable)
                .WithMessage("Booking cannot be cancelled. It may already be cancelled or completed.");
        }

        private async Task<bool> BookingMustExist(int bookingId, CancellationToken cancellationToken)
        {
            return await _bookingRepository.ExistsAsync(bookingId);
        }

        private async Task<bool> BookingMustBeCancellable(CancelBookingCommand command, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(command.BookingId);
            if (booking == null)
                return false;

            var status = booking.Status.ToLowerInvariant();
            return status is "pending" or "confirmed";
        }
    }
}