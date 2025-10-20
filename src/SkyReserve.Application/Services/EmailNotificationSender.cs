using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkyReserve.Application.Interfaces;
using SkyReserve.Domain.Entities;
using SkyReserve.Infrastructure.Email;
using System.Text.Json;

namespace SkyReserve.Application.Services
{
    public class EmailNotificationSender : INotificationSender
    {
        private readonly SESsettings _awsSesSettings;
        private readonly IAmazonSimpleEmailService _sesClient;
        private readonly ILogger<EmailNotificationSender> _logger;

        public EmailNotificationSender(
            IOptions<SESsettings> awsSesSettings,
            IAmazonSimpleEmailService sesClient,
            ILogger<EmailNotificationSender> logger)
        {
            _awsSesSettings = awsSesSettings.Value;
            _sesClient = sesClient;
            _logger = logger;
        }

        public async Task SendAsync(Notification notification)
        {
            if (notification.Channel?.ChannelType != "EMAIL")
            {
                throw new InvalidOperationException("This notification sender only handles EMAIL notifications.");
            }

            try
            {
                var emailData = ExtractEmailDataFromPayload(notification.Payload);

                _logger.LogInformation("Sending email to {Email} via AWS SES", notification.Recipient);

                var sendRequest = new SendEmailRequest
                {
                    Source = _awsSesSettings.FromEmail,
                    Destination = new Destination
                    {
                        ToAddresses = new List<string> { notification.Recipient }
                    },
                    Message = new Message
                    {
                        Subject = new Content(emailData.Subject),
                        Body = new Body
                        {
                            Html = new Content
                            {
                                Charset = "UTF-8",
                                Data = emailData.HtmlMessage
                            },
                            Text = new Content
                            {
                                Charset = "UTF-8",
                                Data = ConvertHtmlToText(emailData.HtmlMessage)
                            }
                        }
                    },
                    ReplyToAddresses = new List<string> { _awsSesSettings.ReplyToEmail ?? _awsSesSettings.FromEmail }
                };

                if (!string.IsNullOrEmpty(_awsSesSettings.ConfigurationSet))
                {
                    sendRequest.ConfigurationSetName = _awsSesSettings.ConfigurationSet;
                }

                var response = await _sesClient.SendEmailAsync(sendRequest);

                _logger.LogInformation("Email sent successfully to {Email}. MessageId: {MessageId}",
                    notification.Recipient, response.MessageId);
            }
            catch (MessageRejectedException ex)
            {
                _logger.LogError(ex, "Email rejected by AWS SES for {Email}. Reason: {Reason}",
                    notification.Recipient, ex.Message);
                throw new InvalidOperationException($"Email was rejected: {ex.Message}", ex);
            }
            catch (MailFromDomainNotVerifiedException ex)
            {
                _logger.LogError(ex, "Domain not verified in AWS SES: {Domain}", _awsSesSettings.FromEmail);
                throw new InvalidOperationException($"Sender domain not verified: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email} via AWS SES", notification.Recipient);
                throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
            }
        }

        private EmailData ExtractEmailDataFromPayload(string payload)
        {
            try
            {
                var payloadData = JsonSerializer.Deserialize<JsonElement>(payload);

                var subject = "SkyReserve Notification";
                var htmlMessage = payload;

                if (payloadData.TryGetProperty("subject", out var subjectElement))
                {
                    subject = subjectElement.GetString() ?? subject;
                }

                if (payloadData.TryGetProperty("htmlMessage", out var htmlElement))
                {
                    htmlMessage = htmlElement.GetString() ?? htmlMessage;
                }
                else if (payloadData.TryGetProperty("body", out var bodyElement))
                {
                    htmlMessage = bodyElement.GetString() ?? htmlMessage;
                }
                else if (payloadData.TryGetProperty("message", out var messageElement))
                {
                    htmlMessage = messageElement.GetString() ?? htmlMessage;
                }
                else if (payloadData.TryGetProperty("content", out var contentElement))
                {
                    htmlMessage = contentElement.GetString() ?? htmlMessage;
                }

                return new EmailData(subject, htmlMessage);
            }
            catch (JsonException)
            {
                return new EmailData("SkyReserve Notification", payload);
            }
        }

        private static string ConvertHtmlToText(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            var text = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
            text = System.Net.WebUtility.HtmlDecode(text);

            return text.Trim();
        }

        private record EmailData(string Subject, string HtmlMessage);
    }
}
