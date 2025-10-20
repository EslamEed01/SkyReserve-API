using Moq;
using SkyReserve.Application.Booking.Commands.Handlers;
using SkyReserve.Application.Booking.Commands.Models;
using SkyReserve.Application.Booking.DTOS;
using SkyReserve.Application.Interfaces;
using SkyReserve.Application.Repository;
using Xunit;

namespace SkyReserve.Test.Booking.Commands
{
    public class ConfirmBookingCommandHandlerTests
    {
        private readonly Mock<IBookingRepository> _mockBookingRepository;
        private readonly Mock<INotificationProducer> _mockNotificationProducer;
        private readonly ConfirmBookingCommandHandler _handler;

        public ConfirmBookingCommandHandlerTests()
        {
            _mockBookingRepository = new Mock<IBookingRepository>();
            _mockNotificationProducer = new Mock<INotificationProducer>();
            _handler = new ConfirmBookingCommandHandler(
                _mockBookingRepository.Object,
                _mockNotificationProducer.Object);
        }

        [Fact]
        public async Task Handle_ValidBookingId_ConfirmsBookingAndSendsNotification()
        {
            // Arrange
            var bookingId = 1;
            var booking = new BookingDto
            {
                BookingId = bookingId,
                UserId = "user123",
                Status = "Pending"
            };

            _mockBookingRepository.Setup(x => x.GetByIdAsync(bookingId))
                .ReturnsAsync(booking);
            _mockBookingRepository.Setup(x => x.UpdateBookingStatusWithTransactionAsync(bookingId, "Confirmed"))
                .ReturnsAsync(true);
            _mockNotificationProducer.Setup(x => x.CreateBookingConfirmationNotificationAsync(
                bookingId, booking.UserId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = new ConfirmBookingCommand { BookingId = bookingId };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            _mockBookingRepository.Verify(x => x.GetByIdAsync(bookingId), Times.Once);
            _mockBookingRepository.Verify(x => x.UpdateBookingStatusWithTransactionAsync(bookingId, "Confirmed"), Times.Once);
            _mockNotificationProducer.Verify(x => x.CreateBookingConfirmationNotificationAsync(
                bookingId, booking.UserId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_BookingNotFound_ReturnsFalse()
        {
            // Arrange
            var bookingId = 999;
            _mockBookingRepository.Setup(x => x.GetByIdAsync(bookingId))
                .ReturnsAsync((BookingDto?)null);

            var command = new ConfirmBookingCommand { BookingId = bookingId };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result);
            _mockBookingRepository.Verify(x => x.GetByIdAsync(bookingId), Times.Once);
            _mockBookingRepository.Verify(x => x.UpdateBookingStatusWithTransactionAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            _mockNotificationProducer.Verify(x => x.CreateBookingConfirmationNotificationAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UpdateFails_ReturnsFalseAndDoesNotSendNotification()
        {
            // Arrange
            var bookingId = 1;
            var booking = new BookingDto
            {
                BookingId = bookingId,
                UserId = "user123",
                Status = "Pending"
            };

            _mockBookingRepository.Setup(x => x.GetByIdAsync(bookingId))
                .ReturnsAsync(booking);
            _mockBookingRepository.Setup(x => x.UpdateBookingStatusWithTransactionAsync(bookingId, "Confirmed"))
                .ReturnsAsync(false);

            var command = new ConfirmBookingCommand { BookingId = bookingId };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result);
            _mockBookingRepository.Verify(x => x.GetByIdAsync(bookingId), Times.Once);
            _mockBookingRepository.Verify(x => x.UpdateBookingStatusWithTransactionAsync(bookingId, "Confirmed"), Times.Once);
            _mockNotificationProducer.Verify(x => x.CreateBookingConfirmationNotificationAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}