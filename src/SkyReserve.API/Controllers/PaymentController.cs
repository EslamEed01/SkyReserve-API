using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyReserve.Application.Booking.Commands.Models;
using SkyReserve.Application.Booking.Queries.Models;
using SkyReserve.Application.DTOs.Payment.DTOs;
using SkyReserve.Application.Interfaces;
using Stripe;
using System.Security.Claims;

namespace SkyReserve.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IStripePayment _stripePayment;
        private readonly ILogger<PaymentsController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMediator _mediator;

        public PaymentsController(
            IStripePayment stripePayment,
            ILogger<PaymentsController> logger,
            IConfiguration configuration,
            IMediator mediator)
        {
            _stripePayment = stripePayment;
            _logger = logger;
            _configuration = configuration;
            _mediator = mediator;
        }

        /// <summary>
        /// Creates a payment intent for processing payment
        /// </summary>       
        [HttpPost("create-payment-intent")]
        public async Task<ActionResult<PaymentIntentResponse>> CreatePaymentIntent(
            [FromBody] CreatePaymentIntentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");
                if (request.Amount < 50 || request.Amount > 99999999)
                {
                    return BadRequest("Amount must be between 50 cents and $999,999.99 (in cents)");
                }

                var response = await _stripePayment.CreatePaymentIntentForOrderAsync(request);

                _logger.LogInformation("Payment intent created for user {UserId}, amount: {Amount} cents",
                    userId, request.Amount);

                return Ok(response);
            }
            catch (StripeException stripeEx)
            {
                _logger.LogError(stripeEx, "Stripe error creating payment intent");
                return BadRequest(new { error = $"Payment processing error: {stripeEx.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment intent");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Handles Stripe webhooks for payment status updates
        /// </summary>
        /// <returns>Empty result</returns>
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var webhookSecret = _configuration["Stripe:WebhookSecret"];

            if (string.IsNullOrEmpty(webhookSecret))
            {
                _logger.LogError("Stripe webhook secret not configured");
                return BadRequest("Webhook not properly configured");
            }

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], webhookSecret);

                _logger.LogInformation("Received Stripe webhook: {EventType}", stripeEvent.Type);

                switch (stripeEvent.Type)
                {
                    case "payment_intent.succeeded":
                        var succeededPaymentIntent = (Stripe.PaymentIntent)stripeEvent.Data.Object;
                        await HandlePaymentSucceeded(succeededPaymentIntent);
                        break;

                    case "payment_intent.canceled":
                        var canceledPaymentIntent = (Stripe.PaymentIntent)stripeEvent.Data.Object;
                        await HandlePaymentCanceled(canceledPaymentIntent);
                        break;
                }

                return Ok();
            }
            catch (StripeException)
            {
                return BadRequest("Invalid signature");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Handles successful payment events
        /// </summary>
        private async Task HandlePaymentSucceeded(Stripe.PaymentIntent paymentIntent)
        {
            try
            {
                _logger.LogInformation("Payment succeeded: {PaymentIntentId}", paymentIntent.Id);

                if (paymentIntent.Metadata.TryGetValue("booking_id", out var bookingIdStr) &&
                    int.TryParse(bookingIdStr, out var bookingId))
                {
                    var getBookingQuery = new GetBookingByIdQuery { BookingId = bookingId };
                    var booking = await _mediator.Send(getBookingQuery);

                    if (booking != null)
                    {
                        var confirmCommand = new ConfirmBookingCommand { BookingId = bookingId };
                        await _mediator.Send(confirmCommand);
                        _logger.LogInformation("Booking {BookingId} payment confirmed", bookingId);
                    }
                    else
                    {
                        _logger.LogWarning("Booking not found for ID: {BookingId}", bookingId);
                    }
                }
                else
                {
                    _logger.LogWarning("No booking information found in payment intent metadata: {PaymentIntentId}", paymentIntent.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payment success for payment intent: {PaymentIntentId}", paymentIntent.Id);
            }
        }

        /// <summary>
        /// Handles canceled payment events
        /// </summary>
        private async Task HandlePaymentCanceled(Stripe.PaymentIntent paymentIntent)
        {
            try
            {
                _logger.LogInformation("Payment canceled: {PaymentIntentId}", paymentIntent.Id);

                if (paymentIntent.Metadata.TryGetValue("booking_id", out var bookingIdStr) &&
                    int.TryParse(bookingIdStr, out var bookingId))
                {
                    var getBookingQuery = new GetBookingByIdQuery { BookingId = bookingId };
                    var booking = await _mediator.Send(getBookingQuery);

                    if (booking != null)
                    {
                        var cancelCommand = new CancelBookingCommand
                        {
                            BookingId = bookingId,
                            CancellationReason = "Payment canceled"
                        };
                        await _mediator.Send(cancelCommand);
                        _logger.LogInformation("Booking {BookingId} canceled due to payment cancellation", bookingId);
                    }
                    else
                    {
                        _logger.LogWarning("Booking not found for ID: {BookingId}", bookingId);
                    }
                }
                else
                {
                    _logger.LogWarning("No booking information found in payment intent metadata: {PaymentIntentId}", paymentIntent.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payment cancellation for payment intent: {PaymentIntentId}", paymentIntent.Id);
            }
        }
    }
}