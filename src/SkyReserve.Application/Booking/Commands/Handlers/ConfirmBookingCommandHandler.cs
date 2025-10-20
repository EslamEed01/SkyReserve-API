using MediatR;
using SkyReserve.Application.Booking.Commands.Models;
using SkyReserve.Application.Interfaces;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Booking.Commands.Handlers
{
    public class ConfirmBookingCommandHandler : IRequestHandler<ConfirmBookingCommand, bool>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly INotificationProducer _notificationProducer;

        public ConfirmBookingCommandHandler(
            IBookingRepository bookingRepository,
            INotificationProducer notificationProducer)
        {
            _bookingRepository = bookingRepository;
            _notificationProducer = notificationProducer;
        }

        public async Task<bool> Handle(ConfirmBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
            {
                return false;
            }

            var success = await _bookingRepository.UpdateBookingStatusWithTransactionAsync(request.BookingId, "Confirmed");

            if (success)
            {
                await _notificationProducer.CreateBookingConfirmationNotificationAsync(
                    request.BookingId,
                    booking.UserId,
                    cancellationToken);
            }

            return success;
        }
    }
}