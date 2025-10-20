using Moq;
using SkyReserve.Application.Booking.DTOS;
using SkyReserve.Application.Booking.Queries.Handlers;
using SkyReserve.Application.Booking.Queries.Models;
using SkyReserve.Application.Repository;
using Xunit;

namespace SkyReserve.Test.Booking.Queries
{
    public class GetBookingByIdQueryHandlerTests
    {
        private readonly Mock<IBookingRepository> _mockBookingRepository;
        private readonly GetBookingByIdQueryHandler _handler;

        public GetBookingByIdQueryHandlerTests()
        {
            _mockBookingRepository = new Mock<IBookingRepository>();
            _handler = new GetBookingByIdQueryHandler(_mockBookingRepository.Object);
        }

        [Fact]
        public async Task Handle_ValidBookingId_ReturnsBookingDto()
        {
            // Arrange
            var bookingId = 1;
            var expectedBooking = new BookingDto
            {
                BookingId = bookingId,
                BookingRef = "ABC123",
                UserId = "user1",
                FlightId = 1,
                Status = "Pending",
                TotalAmount = 100.00m,
                BookingDate = DateTime.UtcNow
            };

            _mockBookingRepository.Setup(x => x.GetByIdAsync(bookingId))
                .ReturnsAsync(expectedBooking);

            var query = new GetBookingByIdQuery { BookingId = bookingId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedBooking.BookingId, result.BookingId);
            Assert.Equal(expectedBooking.BookingRef, result.BookingRef);
            Assert.Equal(expectedBooking.Status, result.Status);
            _mockBookingRepository.Verify(x => x.GetByIdAsync(bookingId), Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidBookingId_ReturnsNull()
        {
            // Arrange
            var bookingId = 999;
            _mockBookingRepository.Setup(x => x.GetByIdAsync(bookingId))
                .ReturnsAsync((BookingDto?)null);

            var query = new GetBookingByIdQuery { BookingId = bookingId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
            _mockBookingRepository.Verify(x => x.GetByIdAsync(bookingId), Times.Once);
        }
    }
}