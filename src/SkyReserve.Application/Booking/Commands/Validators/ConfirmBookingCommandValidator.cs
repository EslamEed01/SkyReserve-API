using FluentValidation;
using SkyReserve.Application.Booking.Commands.Models;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Booking.Commands.Validators
{
    public class ConfirmBookingCommandValidator : AbstractValidator<ConfirmBookingCommand>
    {
        private readonly IBookingRepository _bookingRepository;

        public ConfirmBookingCommandValidator(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;

            RuleFor(x => x.BookingId)
                .GreaterThan(0)
                .WithMessage("Booking ID must be greater than 0.")
                .MustAsync(BookingMustExist)
                .WithMessage("Booking with ID '{PropertyValue}' does not exist.");

            RuleFor(x => x)
                .MustAsync(BookingMustBeConfirmable)
                .WithMessage("Booking cannot be confirmed. It must be in Pending status.");
        }

        private async Task<bool> BookingMustExist(int bookingId, CancellationToken cancellationToken)
        {
            return await _bookingRepository.ExistsAsync(bookingId);
        }

        private async Task<bool> BookingMustBeConfirmable(ConfirmBookingCommand command, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(command.BookingId);
            if (booking == null)
                return false;

            return booking.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase);
        }
    }
}