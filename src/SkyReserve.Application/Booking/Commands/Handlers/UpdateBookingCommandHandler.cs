using MediatR;
using SkyReserve.Application.Booking.Commands.Models;
using SkyReserve.Application.Booking.DTOS;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Booking.Commands.Handlers
{
    public class UpdateBookingCommandHandler : IRequestHandler<UpdateBookingCommand, BookingDto>
    {
        private readonly IBookingRepository _bookingRepository;

        public UpdateBookingCommandHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<BookingDto> Handle(UpdateBookingCommand request, CancellationToken cancellationToken)
        {
            var existingBooking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (existingBooking == null)
                throw new KeyNotFoundException($"Booking with ID {request.BookingId} not found");

            if (!IsValidStatusTransition(existingBooking.Status, request.Status))
                throw new ArgumentException($"Cannot change booking status from {existingBooking.Status} to {request.Status}");

            var updateDto = new UpdateBookingDto
            {
                BookingId = request.BookingId,
                Status = request.Status
            };

            return await _bookingRepository.UpdateAsync(updateDto);
        }

        private static bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            return currentStatus switch
            {
                "Pending" => newStatus is "Confirmed" or "Cancelled",
                "Confirmed" => newStatus is "Completed" or "Cancelled",
                "Completed" => false,
                "Cancelled" => false,
                _ => false
            };
        }
    }
}