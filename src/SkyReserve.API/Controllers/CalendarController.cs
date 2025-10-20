using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyReserve.Application.DTOs;
using SkyReserve.Application.Interfaces;
using SkyReserve.Infrastructure.Authorization;

namespace SkyReserve.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CalendarController : ControllerBase
    {
        private readonly IGoogleCalendarService _calendarService;
        private readonly ILogger<CalendarController> _logger;

        public CalendarController(
            IGoogleCalendarService calendarService,
            ILogger<CalendarController> logger)
        {
            _calendarService = calendarService;
            _logger = logger;
        }

        /// <summary>
        /// Adds a booking to user's Google Calendar
        /// </summary>
        [HttpPost("add-booking")]
        [HasPermission(Permissions.Bookings.ViewOwn)]
        public async Task<ActionResult<BookingCalendarResponse>> AddBookingToCalendar(
            [FromBody] BookingCalendarRequest request,
            [FromHeader(Name = "X-Google-Access-Token")] string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return BadRequest(new BookingCalendarResponse
                {
                    IsSuccess = false,
                    Error = "missing_token",
                    ErrorDescription = "Google access token is required in X-Google-Access-Token header"
                });
            }

            if (string.IsNullOrEmpty(request.BookingRef))
            {
                return BadRequest(new BookingCalendarResponse
                {
                    IsSuccess = false,
                    Error = "missing_booking_ref",
                    ErrorDescription = "Booking reference is required"
                });
            }

            var result = await _calendarService.AddBookingToCalendarAsync(accessToken, request.BookingRef);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully added booking {BookingRef} to calendar", request.BookingRef);
                return Ok(result);
            }

            _logger.LogWarning("Failed to add booking {BookingRef} to calendar: {Error}",
                request.BookingRef, result.ErrorDescription);

            return BadRequest(result);
        }
    }
}