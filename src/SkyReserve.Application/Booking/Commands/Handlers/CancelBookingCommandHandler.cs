using MediatR;
using SkyReserve.Application.Booking.Commands.Models;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Booking.Commands.Handlers
{
    public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, bool>
    {
        private readonly IBookingRepository _bookingRepository;

        public CancelBookingCommandHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<bool> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
                return false;

            if (booking.Status == "Cancelled")
                throw new InvalidOperationException("Booking is already cancelled");

            if (booking.Status == "Completed")
                throw new InvalidOperationException("Cannot cancel a completed booking");

            return await _bookingRepository.CancelBookingWithRefundAsync(request.BookingId, request.CancellationReason);
        }
    }
}