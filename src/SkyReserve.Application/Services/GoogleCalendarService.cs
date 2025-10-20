using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkyReserve.Application.DTOs;
using SkyReserve.Application.Interfaces;
using SkyReserve.Application.Repository;
using SkyReserve.Application.Settings;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SkyReserve.Infrastructure.Services
{
    public class GoogleCalendarService : IGoogleCalendarService
    {
        private readonly GoogleCalendarSettings _settings;
        private readonly ILogger<GoogleCalendarService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IBookingRepository _bookingRepository;

        public GoogleCalendarService(
            IOptions<GoogleCalendarSettings> settings,
            ILogger<GoogleCalendarService> logger,
            HttpClient httpClient,
            IBookingRepository bookingRepository)
        {
            _settings = settings.Value;
            _logger = logger;
            _httpClient = httpClient;
            _bookingRepository = bookingRepository;
        }

        public async Task<CalendarEventResponse> CreateEventAsync(string accessToken, CreateCalendarEventRequest request)
        {
            try
            {
                var url = $"{_settings.ApiBaseUrl}/calendars/{_settings.DefaultCalendarId}/events";

                var eventData = new
                {
                    summary = request.Summary,
                    description = request.Description,
                    location = request.Location,
                    start = new
                    {
                        dateTime = request.StartDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        timeZone = _settings.TimeZone
                    },
                    end = new
                    {
                        dateTime = request.EndDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        timeZone = _settings.TimeZone
                    },
                    attendees = request.AttendeeEmails.Select(email => new { email }).ToArray()
                };

                var json = JsonSerializer.Serialize(eventData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                requestMessage.Content = content;

                var response = await _httpClient.SendAsync(requestMessage);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

                    return new CalendarEventResponse
                    {
                        IsSuccess = true,
                        EventId = result.GetProperty("id").GetString(),
                        EventUrl = result.GetProperty("htmlLink").GetString()
                    };
                }

                _logger.LogError("Failed to create calendar event. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, responseContent);

                return new CalendarEventResponse
                {
                    IsSuccess = false,
                    Error = "creation_failed",
                    ErrorDescription = $"Failed to create event: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating calendar event");
                return new CalendarEventResponse
                {
                    IsSuccess = false,
                    Error = "internal_error",
                    ErrorDescription = ex.Message
                };
            }
        }

        public async Task<CalendarEventsResponse> GetEventsAsync(string accessToken, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var url = $"{_settings.ApiBaseUrl}/calendars/{_settings.DefaultCalendarId}/events";
                var queryParams = new List<string> { "singleEvents=true", "orderBy=startTime" };

                if (startDate.HasValue)
                    queryParams.Add($"timeMin={startDate.Value:yyyy-MM-ddTHH:mm:ss.fffZ}");

                if (endDate.HasValue)
                    queryParams.Add($"timeMax={endDate.Value:yyyy-MM-ddTHH:mm:ss.fffZ}");

                url += "?" + string.Join("&", queryParams);

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var events = new List<CalendarEvent>();

                    if (result.TryGetProperty("items", out var items))
                    {
                        foreach (var item in items.EnumerateArray())
                        {
                            var calendarEvent = new CalendarEvent
                            {
                                Id = item.GetProperty("id").GetString() ?? "",
                                Summary = item.TryGetProperty("summary", out var summary) ? summary.GetString() ?? "" : "",
                                Description = item.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "",
                                Location = item.TryGetProperty("location", out var loc) ? loc.GetString() ?? "" : "",
                                Status = item.TryGetProperty("status", out var status) ? status.GetString() ?? "" : ""
                            };

                            // Parse start time
                            if (item.TryGetProperty("start", out var start))
                            {
                                if (start.TryGetProperty("dateTime", out var startDateTime))
                                {
                                    DateTime.TryParse(startDateTime.GetString(), out var parsedStart);
                                    calendarEvent.Start = parsedStart;
                                }
                            }

                            if (item.TryGetProperty("end", out var end))
                            {
                                if (end.TryGetProperty("dateTime", out var endDateTime))
                                {
                                    DateTime.TryParse(endDateTime.GetString(), out var parsedEnd);
                                    calendarEvent.End = parsedEnd;
                                }
                            }

                            events.Add(calendarEvent);
                        }
                    }

                    return new CalendarEventsResponse
                    {
                        IsSuccess = true,
                        Events = events
                    };
                }

                return new CalendarEventsResponse
                {
                    IsSuccess = false,
                    Error = "fetch_failed",
                    ErrorDescription = $"Failed to get events: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting calendar events");
                return new CalendarEventsResponse
                {
                    IsSuccess = false,
                    Error = "internal_error",
                    ErrorDescription = ex.Message
                };
            }
        }

        public async Task<CalendarEventResponse> DeleteEventAsync(string accessToken, string eventId)
        {
            try
            {
                var url = $"{_settings.ApiBaseUrl}/calendars/{_settings.DefaultCalendarId}/events/{eventId}";

                using var request = new HttpRequestMessage(HttpMethod.Delete, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);

                return new CalendarEventResponse
                {
                    IsSuccess = response.IsSuccessStatusCode,
                    EventId = response.IsSuccessStatusCode ? eventId : null,
                    Error = response.IsSuccessStatusCode ? null : "deletion_failed",
                    ErrorDescription = response.IsSuccessStatusCode ? null : $"Failed to delete event: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting calendar event");
                return new CalendarEventResponse
                {
                    IsSuccess = false,
                    Error = "internal_error",
                    ErrorDescription = ex.Message
                };
            }
        }

        public async Task<CalendarEventResponse> CreateFlightEventAsync(string accessToken, string flightNumber, DateTime departureTime, DateTime arrivalTime, string from, string to)
        {
            var request = new CreateCalendarEventRequest
            {
                Summary = $"Flight {flightNumber}",
                Description = $"Flight from {from} to {to}\nFlight Number: {flightNumber}",
                StartDateTime = departureTime,
                EndDateTime = arrivalTime,
                Location = $"{from} Airport"
            };

            return await CreateEventAsync(accessToken, request);
        }

        public async Task<BookingCalendarResponse> AddBookingToCalendarAsync(string accessToken, string bookingRef)
        {
            try
            {
                var booking = await _bookingRepository.GetByBookingRefAsync(bookingRef);
                if (booking == null)
                {
                    return new BookingCalendarResponse
                    {
                        IsSuccess = false,
                        Error = "booking_not_found",
                        ErrorDescription = "Booking not found with the provided reference"
                    };
                }

                var eventRequest = new CreateCalendarEventRequest
                {
                    Summary = $"✈️ Flight {booking.FlightNumber} - {booking.DepartureAirportCode} to {booking.ArrivalAirportCode}",
                    Description = $"🎫 Booking Reference: {booking.BookingRef}\n" +
                                 $"✈️ Flight: {booking.FlightNumber}\n" +
                                 $"🛫 Departure: {booking.DepartureAirportName} ({booking.DepartureAirportCode})\n" +
                                 $"🛬 Arrival: {booking.ArrivalAirportName} ({booking.ArrivalAirportCode})\n" +
                                 $"💺 Class: {booking.FareClass}\n" +
                                 $"💰 Total Amount: {booking.TotalAmount} {booking.Currency}\n" +
                                 $"📋 Status: {booking.Status}",
                    StartDateTime = booking.DepartureTime,
                    EndDateTime = booking.ArrivalTime,
                    Location = $"{booking.DepartureAirportName} ({booking.DepartureAirportCode})",
                    AttendeeEmails = !string.IsNullOrEmpty(booking.UserEmail) ? [booking.UserEmail] : []
                };

                var result = await CreateEventAsync(accessToken, eventRequest);

                if (result.IsSuccess)
                {
                    return new BookingCalendarResponse
                    {
                        IsSuccess = true,
                        EventId = result.EventId,
                        EventUrl = result.EventUrl,
                        BookingInfo = new BookingCalendarInfo
                        {
                            BookingRef = booking.BookingRef,
                            FlightNumber = booking.FlightNumber,
                            DepartureTime = booking.DepartureTime,
                            ArrivalTime = booking.ArrivalTime,
                            From = $"{booking.DepartureAirportName} ({booking.DepartureAirportCode})",
                            To = $"{booking.ArrivalAirportName} ({booking.ArrivalAirportCode})"
                        }
                    };
                }

                return new BookingCalendarResponse
                {
                    IsSuccess = false,
                    Error = result.Error,
                    ErrorDescription = result.ErrorDescription
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding booking to calendar for booking ref: {BookingRef}", bookingRef);
                return new BookingCalendarResponse
                {
                    IsSuccess = false,
                    Error = "internal_error",
                    ErrorDescription = ex.Message
                };
            }
        }




    }
}