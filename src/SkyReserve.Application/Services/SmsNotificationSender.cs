using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkyReserve.Application.Interfaces;
using SkyReserve.Domain.Entities;
using SkyReserve.Infrastructure.SMS;
using System.Text.Json;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace SkyReserve.Application.Services
{
    public class SmsNotificationSender : INotificationSender
    {
        private readonly TwilioSettings _twilioSettings;
        private readonly ILogger<SmsNotificationSender> _logger;

        public SmsNotificationSender(
            IOptions<TwilioSettings> twilioSettings,
            ILogger<SmsNotificationSender> logger)
        {
            _twilioSettings = twilioSettings.Value;
            _logger = logger;

            TwilioClient.Init(_twilioSettings.AccountSid, _twilioSettings.Secret);
        }

        public async Task SendAsync(Notification notification)
        {
            if (notification.Channel?.ChannelType != "SMS")
            {
                throw new InvalidOperationException("This notification sender only handles SMS notifications.");
            }

            try
            {
                var messageBody = ExtractMessageFromPayload(notification.Payload);

                _logger.LogInformation("Sending SMS to {PhoneNumber} via Twilio", notification.Recipient);

                var messageResource = await MessageResource.CreateAsync(
                    body: messageBody,
                    from: new PhoneNumber(_twilioSettings.FromPhone),
                    to: new PhoneNumber(notification.Recipient)
                );

                _logger.LogInformation("SMS sent successfully to {PhoneNumber}. MessageSid: {MessageSid}",
                    notification.Recipient, messageResource.Sid);
            }
            catch (Twilio.Exceptions.ApiException ex)
            {
                _logger.LogError(ex, "Twilio API error when sending SMS to {PhoneNumber}. Code: {Code}, Message: {Message}",
                    notification.Recipient, ex.Code, ex.Message);
                throw new InvalidOperationException($"SMS API error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS to {PhoneNumber} via Twilio", notification.Recipient);
                throw new InvalidOperationException($"Failed to send SMS: {ex.Message}", ex);
            }
        }

        private string ExtractMessageFromPayload(string payload)
        {
            try
            {
                var payloadData = JsonSerializer.Deserialize<JsonElement>(payload);

                if (payloadData.TryGetProperty("message", out var messageElement))
                {
                    return messageElement.GetString() ?? payload;
                }

                if (payloadData.TryGetProperty("body", out var bodyElement))
                {
                    return bodyElement.GetString() ?? payload;
                }

                if (payloadData.TryGetProperty("text", out var textElement))
                {
                    return textElement.GetString() ?? payload;
                }

                if (payloadData.TryGetProperty("type", out var typeElement) &&
                    typeElement.GetString() == "welcome" &&
                    payloadData.TryGetProperty("firstName", out var firstNameElement))
                {
                    return GenerateWelcomeMessage(firstNameElement.GetString() ?? "User");
                }

                return payload;
            }
            catch (JsonException)
            {
                return payload;
            }
        }

        private static string GenerateWelcomeMessage(string firstName)
        {
            var timeOfDay = DateTime.Now.Hour switch
            {
                >= 5 and < 12 => "Good morning",
                >= 12 and < 17 => "Good afternoon",
                >= 17 and < 21 => "Good evening",
                _ => "Hello"
            };

            return $"{timeOfDay}, {firstName}! Welcome back to SkyReserve. You've successfully logged in. Have a great flight experience! ✈️";
        }
    }
}
