using Moq;
using SkyReserve.Application.Booking.DTOS;
using SkyReserve.Application.Booking.Queries.Handlers;
using SkyReserve.Application.Booking.Queries.Models;
using SkyReserve.Application.Repository;
using Xunit;

namespace SkyReserve.Test.Booking.Queries
{
    public class GetUserBookingsQueryHandlerTests
    {
        private readonly Mock<IBookingRepository> _mockBookingRepository;
        private readonly GetUserBookingsQueryHandler _handler;

        public GetUserBookingsQueryHandlerTests()
        {
            _mockBookingRepository = new Mock<IBookingRepository>();
            _handler = new GetUserBookingsQueryHandler(_mockBookingRepository.Object);
        }

        [Fact]
        public async Task Handle_ValidUserId_ReturnsUserBookings()
        {
            // Arrange
            var userId = "user123";
            var expectedBookings = new List<BookingDto>
            {
                new BookingDto { BookingId = 1, UserId = userId, Status = "Confirmed" },
                new BookingDto { BookingId = 2, UserId = userId, Status = "Pending" }
            };

            _mockBookingRepository.Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(expectedBookings);

            var query = new GetUserBookingsQuery { UserId = userId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, booking => Assert.Equal(userId, booking.UserId));
            _mockBookingRepository.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task Handle_UserWithNoBookings_ReturnsEmptyCollection()
        {
            // Arrange
            var userId = "user123";
            var expectedBookings = new List<BookingDto>();

            _mockBookingRepository.Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(expectedBookings);

            var query = new GetUserBookingsQuery { UserId = userId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockBookingRepository.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
        }
    }
}