using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyReserve.Application.Booking.Commands.Models;
using SkyReserve.Application.Booking.DTOS;
using SkyReserve.Application.Booking.Queries.Models;
using SkyReserve.API.Models;
using System.Security.Claims;
using SkyReserve.Infrastructure.Authorization;

namespace SkyReserve.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BookingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a simple booking (single passenger) - Authenticated users only
        /// </summary>
        [HttpPost]
        [HasPermission(Permissions.Bookings.Create)]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingCommand request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                request.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

                var result = await _mediator.Send(request);
                return CreatedAtAction(nameof(GetBooking), new { id = result.BookingId }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }


        /// <summary>
        /// Create a guest booking - No authentication required
        /// </summary>
        [HttpPost("guest")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateGuestBooking([FromBody] CreateGuestBookingCommand request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _mediator.Send(request);
                return CreatedAtAction(nameof(GetGuestBooking),
                    new { bookingRef = result.BookingRef, lastName = request.Passengers.First().LastName },
                    result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get a specific booking by ID - Authenticated users only
        /// </summary>
        [HttpGet("{id}")]
        [HasPermission(Permissions.Bookings.ViewOwn)]
        public async Task<IActionResult> GetBooking(int id)
        {
            var query = new GetBookingByIdQuery { BookingId = id };
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound($"Booking with ID {id} not found");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (result.UserId != userId && !User.IsInRole("Admin"))
                return Forbid("You don't have access to this booking");

            return Ok(result);
        }

        /// <summary>
        /// Get guest booking by reference and last name - No authentication required
        /// </summary>
        [HttpGet("guest")]
        [AllowAnonymous]
        public async Task<IActionResult> GetGuestBooking([FromQuery] string bookingRef, [FromQuery] string lastName)
        {
            try
            {
                if (string.IsNullOrEmpty(bookingRef) || string.IsNullOrEmpty(lastName))
                    return BadRequest("Booking reference and last name are required");

                var query = new GetGuestBookingQuery
                {
                    BookingRef = bookingRef,
                    LastName = lastName
                };

                var result = await _mediator.Send(query);

                if (result == null)
                    return NotFound("Guest booking not found or last name doesn't match");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get booking by booking reference - Authenticated users only
        /// </summary>
        [HttpGet("reference/{bookingRef}")]
        [HasPermission(Permissions.Bookings.ViewOwn)]
        public async Task<IActionResult> GetBookingByReference(string bookingRef)
        {
            var query = new GetBookingByRefQuery { BookingRef = bookingRef };
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound($"Booking with reference {bookingRef} not found");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (result.UserId != userId && !User.IsInRole("Admin"))
                return Forbid("You don't have access to this booking");

            return Ok(result);
        }

        /// <summary>
        /// Get all bookings for the current user
        /// </summary>
        [HttpGet("my-bookings")]
        [HasPermission(Permissions.Bookings.ViewOwn)]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User ID not found");

            var query = new GetUserBookingsQuery { UserId = userId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get bookings for a specific user (Admin only)
        /// </summary>
        [HttpGet("user/{userId}")]
        [HasPermission(Permissions.Bookings.ViewAll)]
        public async Task<IActionResult> GetUserBookings(string userId)
        {
            var query = new GetUserBookingsQuery { UserId = userId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get bookings for a specific flight (Admin only)
        /// </summary>
        [HttpGet("flight/{flightId}")]
        [HasPermission(Permissions.Bookings.ViewAll)]
        public async Task<IActionResult> GetFlightBookings(int flightId)
        {
            var query = new GetFlightBookingsQuery { FlightId = flightId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Update a booking
        /// </summary>
        [HttpPut("{id}")]
        [HasPermission(Permissions.Bookings.UpdateOwn)]
        public async Task<IActionResult> UpdateBooking(int id, [FromBody] UpdateBookingRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var getQuery = new GetBookingByIdQuery { BookingId = id };
                var existingBooking = await _mediator.Send(getQuery);

                if (existingBooking == null)
                    return NotFound($"Booking with ID {id} not found");

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (existingBooking.UserId != userId && !User.IsInRole("Admin"))
                    return Forbid("You don't have access to this booking");

                var command = new UpdateBookingCommand
                {
                    BookingId = id,
                    Status = request.Status
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Cancel a booking
        /// </summary>
        [HttpPost("{id}/cancel")]
        [HasPermission(Permissions.Bookings.CancelOwn)]
        public async Task<IActionResult> CancelBooking(int id, [FromBody] CancelBookingRequest request)
        {
            try
            {
                var getQuery = new GetBookingByIdQuery { BookingId = id };
                var existingBooking = await _mediator.Send(getQuery);

                if (existingBooking == null)
                    return NotFound($"Booking with ID {id} not found");

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (existingBooking.UserId != userId && !User.IsInRole("Admin"))
                    return Forbid("You don't have access to this booking");

                var command = new CancelBookingCommand
                {
                    BookingId = id,
                    CancellationReason = request.CancellationReason
                };

                var result = await _mediator.Send(command);

                if (result)
                    return Ok(new { message = "Booking cancelled successfully" });

                return BadRequest("Failed to cancel booking");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Cancel a guest booking - No authentication required
        /// </summary>
        [HttpPost("guest/cancel")]
        [AllowAnonymous]
        public async Task<IActionResult> CancelGuestBooking([FromBody] CancelGuestBookingRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var getQuery = new GetGuestBookingQuery
                {
                    BookingRef = request.BookingRef,
                    LastName = request.LastName
                };
                var existingBooking = await _mediator.Send(getQuery);

                if (existingBooking == null)
                    return NotFound("Guest booking not found or last name doesn't match");

                var command = new CancelBookingCommand
                {
                    BookingId = existingBooking.BookingId,
                    CancellationReason = request.CancellationReason
                };

                var result = await _mediator.Send(command);

                if (result)
                    return Ok(new { message = "Guest booking cancelled successfully" });

                return BadRequest("Failed to cancel guest booking");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Confirm a booking (Admin only)
        /// </summary>
        [HttpPost("{id}/confirm")]
        [HasPermission(Permissions.Bookings.ManageAll)]
        public async Task<IActionResult> ConfirmBooking(int id)
        {
            try
            {
                var command = new ConfirmBookingCommand { BookingId = id };
                var result = await _mediator.Send(command);

                if (result)
                    return Ok(new { message = "Booking confirmed successfully" });

                return BadRequest("Failed to confirm booking");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Validate a booking
        /// </summary>
        [HttpGet("{id}/validate")]
        [HasPermission(Permissions.Bookings.ViewOwn)]
        public async Task<IActionResult> ValidateBooking(int id)
        {
            var getQuery = new GetBookingByIdQuery { BookingId = id };
            var existingBooking = await _mediator.Send(getQuery);

            if (existingBooking == null)
                return NotFound($"Booking with ID {id} not found");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (existingBooking.UserId != userId && !User.IsInRole("Admin"))
                return Forbid("You don't have access to this booking");

            var query = new ValidateBookingQuery { BookingId = id };
            var isValid = await _mediator.Send(query);
            return Ok(new { isValid });
        }

        /// <summary>
        /// Calculate booking total for a flight and passenger count
        /// </summary>
        [HttpGet("calculate-total")]
        [AllowAnonymous]
        public async Task<IActionResult> CalculateBookingTotal([FromQuery] int flightId, [FromQuery] int passengerCount = 1)
        {
            try
            {
                if (passengerCount <= 0)
                    return BadRequest("Passenger count must be greater than 0");

                var query = new CalculateBookingTotalQuery
                {
                    FlightId = flightId,
                    PassengerCount = passengerCount
                };

                var total = await _mediator.Send(query);
                return Ok(new { flightId, passengerCount, totalAmount = total });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
