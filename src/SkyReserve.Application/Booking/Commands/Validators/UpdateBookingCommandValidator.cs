using FluentValidation;
using SkyReserve.Application.Booking.Commands.Models;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Booking.Commands.Validators
{
    public class UpdateBookingCommandValidator : AbstractValidator<UpdateBookingCommand>
    {
        private readonly IBookingRepository _bookingRepository;

        public UpdateBookingCommandValidator(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;

            RuleFor(x => x.BookingId)
                .GreaterThan(0)
                .WithMessage("Booking ID must be greater than 0.")
                .MustAsync(BookingMustExist)
                .WithMessage("Booking with ID '{PropertyValue}' does not exist.");

            RuleFor(x => x.Status)
                .NotEmpty()
                .WithMessage("Status is required.")
                .Must(BeValidStatus)
                .WithMessage("Status must be one of: Pending, Confirmed, Completed, Cancelled.");

            RuleFor(x => x)
                .MustAsync(StatusTransitionMustBeValid)
                .WithMessage("Invalid status transition. Check current status and allowed transitions.");
        }

        private async Task<bool> BookingMustExist(int bookingId, CancellationToken cancellationToken)
        {
            return await _bookingRepository.ExistsAsync(bookingId);
        }

        private static bool BeValidStatus(string status)
        {
            var validStatuses = new[] { "Pending", "Confirmed", "Completed", "Cancelled" };
            return validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
        }

        private async Task<bool> StatusTransitionMustBeValid(UpdateBookingCommand command, CancellationToken cancellationToken)
        {
            var existingBooking = await _bookingRepository.GetByIdAsync(command.BookingId);
            if (existingBooking == null)
                return false;

            return IsValidStatusTransition(existingBooking.Status, command.Status);
        }

        private static bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            return currentStatus.ToLowerInvariant() switch
            {
                "pending" => newStatus.ToLowerInvariant() is "confirmed" or "cancelled",
                "confirmed" => newStatus.ToLowerInvariant() is "completed" or "cancelled",
                "completed" => false,
                "cancelled" => false,
                _ => false
            };
        }
    }
}