using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyReserve.Application.Passenger.Commands.Models;
using SkyReserve.Application.Passenger.DTOS;
using SkyReserve.Application.Passenger.Queries.Models;

namespace SkyReserve.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PassengerController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PassengerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Track guest booking using booking reference and last name
        /// </summary>
        [HttpPost("guest-tracking")]
        [AllowAnonymous]
        public async Task<IActionResult> TrackGuestBooking([FromBody] GuestTrackingDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var query = new GetGuestBookingDetailsQuery
            {
                BookingRef = request.BookingRef,
                LastName = request.LastName
            };

            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound("Booking not found or invalid credentials");

            return Ok(result);
        }

        /// <summary>
        /// Get a specific passenger by id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPassenger(int id)
        {
            var query = new GetPassengerByIdQuery { PassengerId = id };
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound($"Passenger with ID {id} not found");

            return Ok(result);
        }

        /// <summary>
        /// Get all passengers with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPassengers([FromQuery] GetAllPassengersQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get passengers by booking ID
        /// </summary>
        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetPassengersByBooking(int bookingId)
        {
            var query = new GetPassengersByBookingIdQuery { BookingId = bookingId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get passengers by passport number
        /// </summary>
        [HttpGet("passport/{passportNumber}")]
        public async Task<IActionResult> GetPassengersByPassport(string passportNumber)
        {
            var query = new GetPassengersByPassportNumberQuery { PassportNumber = passportNumber };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Create a new passenger
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePassenger([FromBody] CreatePassengerCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetPassenger), new { id = result.PassengerId }, result);
        }

        /// <summary>
        /// Update an existing passenger
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePassenger(int id, [FromBody] UpdatePassengerCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            command.PassengerId = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Delete a passenger
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePassenger(int id)
        {
            var command = new DeletePassengerCommand { PassengerId = id };
            var result = await _mediator.Send(command);

            if (result)
                return NoContent();

            return NotFound($"Passenger with ID {id} not found or could not be deleted");
        }
    }
}