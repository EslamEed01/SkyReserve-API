using Moq;
using SkyReserve.Application.Booking.DTOS;
using SkyReserve.Application.Booking.Queries.Handlers;
using SkyReserve.Application.Booking.Queries.Models;
using SkyReserve.Application.Passenger.DTOS;
using SkyReserve.Application.Repository;
using Xunit;

namespace SkyReserve.Test.Booking.Queries
{
    public class GetGuestBookingQueryHandlerTests
    {
        private readonly Mock<IBookingRepository> _mockBookingRepository;
        private readonly GetGuestBookingQueryHandler _handler;

        public GetGuestBookingQueryHandlerTests()
        {
            _mockBookingRepository = new Mock<IBookingRepository>();
            _handler = new GetGuestBookingQueryHandler(_mockBookingRepository.Object);
        }

        [Fact]
        public async Task Handle_ValidGuestBooking_ReturnsBookingDto()
        {
            // Arrange
            var bookingRef = "ABC123";
            var lastName = "Smith";
            var guestBooking = new BookingDto
            {
                BookingId = 1,
                BookingRef = bookingRef,
                UserId = null, // Guest booking
                FlightId = 1,
                Status = "Pending",
                Passengers = new List<PassengerDto>
                {
                    new PassengerDto { LastName = lastName, FirstName = "John" }
                }
            };

            _mockBookingRepository.Setup(x => x.GetByBookingRefAsync(bookingRef))
                .ReturnsAsync(guestBooking);

            var query = new GetGuestBookingQuery { BookingRef = bookingRef, LastName = lastName };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(guestBooking.BookingId, result.BookingId);
            Assert.Equal(guestBooking.BookingRef, result.BookingRef);
            _mockBookingRepository.Verify(x => x.GetByBookingRefAsync(bookingRef), Times.Once);
        }

        [Fact]
        public async Task Handle_BookingNotFound_ReturnsNull()
        {
            // Arrange
            var bookingRef = "NOTFOUND";
            var lastName = "Smith";
            _mockBookingRepository.Setup(x => x.GetByBookingRefAsync(bookingRef))
                .ReturnsAsync((BookingDto?)null);

            var query = new GetGuestBookingQuery { BookingRef = bookingRef, LastName = lastName };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
            _mockBookingRepository.Verify(x => x.GetByBookingRefAsync(bookingRef), Times.Once);
        }

        [Fact]
        public async Task Handle_AuthenticatedUserBooking_ReturnsNull()
        {
            // Arrange
            var bookingRef = "ABC123";
            var lastName = "Smith";
            var authenticatedUserBooking = new BookingDto
            {
                BookingId = 1,
                BookingRef = bookingRef,
                UserId = "user123",
                FlightId = 1,
                Status = "Pending"
            };

            _mockBookingRepository.Setup(x => x.GetByBookingRefAsync(bookingRef))
                .ReturnsAsync(authenticatedUserBooking);

            var query = new GetGuestBookingQuery { BookingRef = bookingRef, LastName = lastName };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
            _mockBookingRepository.Verify(x => x.GetByBookingRefAsync(bookingRef), Times.Once);
        }

        [Fact]
        public async Task Handle_LastNameMismatch_ReturnsNull()
        {
            // Arrange
            var bookingRef = "ABC123";
            var lastName = "Johnson";
            var guestBooking = new BookingDto
            {
                BookingId = 1,
                BookingRef = bookingRef,
                UserId = null,
                FlightId = 1,
                Status = "Pending",
                Passengers = new List<PassengerDto>
                {
                    new PassengerDto { LastName = "Smith", FirstName = "John" }
                }
            };

            _mockBookingRepository.Setup(x => x.GetByBookingRefAsync(bookingRef))
                .ReturnsAsync(guestBooking);

            var query = new GetGuestBookingQuery { BookingRef = bookingRef, LastName = lastName };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
            _mockBookingRepository.Verify(x => x.GetByBookingRefAsync(bookingRef), Times.Once);
        }
    }
}