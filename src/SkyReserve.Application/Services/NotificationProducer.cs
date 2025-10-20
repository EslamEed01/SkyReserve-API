using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkyReserve.Application.Interfaces;
using SkyReserve.Application.Repository;
using SkyReserve.Application.Settings;
using SkyReserve.Domain.Entities;
using System.Text.Json;

namespace SkyReserve.Application.Services
{


    public class NotificationProducer : INotificationProducer
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAmazonSQS _sqsClient;
        private readonly SqsSettings _sqsSettings;
        private readonly ITemplateService _templateService;
        private readonly ILogger<NotificationProducer> _logger;

        public NotificationProducer(
            INotificationRepository notificationRepository,
            IBookingRepository bookingRepository,
            IUserRepository userRepository,
            UserManager<ApplicationUser> userManager,
            IAmazonSQS sqsClient,
            IOptions<SqsSettings> sqsSettings,
            ITemplateService templateService,
            ILogger<NotificationProducer> logger)
        {
            _notificationRepository = notificationRepository;
            _bookingRepository = bookingRepository;
            _userRepository = userRepository;
            _userManager = userManager;
            _sqsClient = sqsClient;
            _sqsSettings = sqsSettings.Value;
            _templateService = templateService;
            _logger = logger;
        }

        public async Task CreateBookingConfirmationNotificationAsync(int bookingId, string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    _logger.LogWarning("Booking not found: {BookingId}", bookingId);
                    return;
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return;
                }

                var emailChannel = await GetOrCreateEmailChannelAsync(cancellationToken);
                var smsChannel = await GetOrCreateSmsChannelAsync(cancellationToken);

                if (!string.IsNullOrEmpty(user.Email))
                {
                    var emailNotification = await CreateEmailNotificationAsync(booking, user, emailChannel.ChannelId, cancellationToken);
                    await SendNotificationToSqsAsync(emailNotification.NotificationId, cancellationToken);
                }

                if (!string.IsNullOrEmpty(user.PhoneNumber))
                {
                    var smsNotification = await CreateSmsNotificationAsync(booking, user, smsChannel.ChannelId, cancellationToken);
                    await SendNotificationToSqsAsync(smsNotification.NotificationId, cancellationToken);
                }

                _logger.LogInformation("Created booking confirmation notifications for booking {BookingId}", bookingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking confirmation notifications for booking {BookingId}", bookingId);
                throw;
            }
        }

        private async Task<Notification> CreateEmailNotificationAsync(
            SkyReserve.Application.Booking.DTOS.BookingDto booking,
            ApplicationUser user,
            int channelId,
            CancellationToken cancellationToken)
        {
            var templateParameters = new Dictionary<string, string>
            {
                { "firstName", user.FirstName },
                { "bookingRef", booking.BookingRef },
                { "flightId", booking.FlightId.ToString() },
                { "totalAmount", booking.TotalAmount.ToString("F2") },
                { "bookingDate", booking.BookingDate.ToString("yyyy-MM-dd") }
            };

            var htmlMessage = await _templateService.GetEmailTemplateAsync("BookingConfirmation", templateParameters);

            var emailPayload = new
            {
                subject = "Booking Confirmation - SkyReserve",
                htmlMessage = htmlMessage,
                bookingRef = booking.BookingRef,
                amount = booking.TotalAmount,
                flightId = booking.FlightId
            };

            var notification = new Notification
            {
                ChannelId = channelId,
                Recipient = user.Email!,
                UserId = user.Id,
                Payload = JsonSerializer.Serialize(emailPayload),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            return await _notificationRepository.CreateAsync(notification, cancellationToken);
        }

        private async Task<Notification> CreateSmsNotificationAsync(
            SkyReserve.Application.Booking.DTOS.BookingDto booking,
            ApplicationUser user,
            int channelId,
            CancellationToken cancellationToken)
        {
            var templateParameters = new Dictionary<string, string>
            {
                { "bookingRef", booking.BookingRef },
                { "totalAmount", booking.TotalAmount.ToString("F2") }
            };

            var message = await _templateService.GetSmsTemplateAsync("BookingConfirmation", templateParameters);

            var smsPayload = new
            {
                message = message,
                bookingRef = booking.BookingRef,
                amount = booking.TotalAmount
            };

            var notification = new Notification
            {
                ChannelId = channelId,
                Recipient = user.PhoneNumber!,
                UserId = user.Id,
                Payload = JsonSerializer.Serialize(smsPayload),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            return await _notificationRepository.CreateAsync(notification, cancellationToken);
        }

        public async Task CreateLoginNotificationAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return;
                }

                var emailChannel = await GetOrCreateEmailChannelAsync(cancellationToken);
                var smsChannel = await GetOrCreateSmsChannelAsync(cancellationToken);

                if (!string.IsNullOrEmpty(user.Email))
                {
                    var emailNotification = await CreateLoginEmailNotificationAsync(user, emailChannel.ChannelId, cancellationToken);
                    await SendNotificationToSqsAsync(emailNotification.NotificationId, cancellationToken);
                }

                if (!string.IsNullOrEmpty(user.PhoneNumber))
                {
                    var smsNotification = await CreateLoginSmsNotificationAsync(user, smsChannel.ChannelId, cancellationToken);
                    await SendNotificationToSqsAsync(smsNotification.NotificationId, cancellationToken);
                }

                _logger.LogInformation("Created login notifications for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating login notifications for user {UserId}", userId);
                throw;
            }
        }

        public async Task CreateRegistrationWelcomeNotificationAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return;
                }

                var emailChannel = await GetOrCreateEmailChannelAsync(cancellationToken);
                var smsChannel = await GetOrCreateSmsChannelAsync(cancellationToken);

                if (!string.IsNullOrEmpty(user.Email))
                {
                    var emailNotification = await CreateWelcomeEmailNotificationAsync(user, emailChannel.ChannelId, cancellationToken);
                    await SendNotificationToSqsAsync(emailNotification.NotificationId, cancellationToken);
                }

                if (!string.IsNullOrEmpty(user.PhoneNumber))
                {
                    var smsNotification = await CreateWelcomeSmsNotificationAsync(user, smsChannel.ChannelId, cancellationToken);
                    await SendNotificationToSqsAsync(smsNotification.NotificationId, cancellationToken);
                }

                _logger.LogInformation("Created registration welcome notifications for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating registration welcome notifications for user {UserId}", userId);
                throw;
            }
        }

        private async Task<Notification> CreateLoginEmailNotificationAsync(
            ApplicationUser user,
            int channelId,
            CancellationToken cancellationToken)
        {
            var loginTime = DateTime.UtcNow;
            var templateParameters = new Dictionary<string, string>
            {
                { "firstName", user.FirstName },
                { "loginTime", loginTime.ToString("yyyy-MM-dd HH:mm:ss") }
            };

            var htmlMessage = await _templateService.GetEmailTemplateAsync("LoginNotification", templateParameters);

            var emailPayload = new
            {
                subject = "Login Notification - SkyReserve",
                htmlMessage = htmlMessage,
                loginTime = loginTime,
                userId = user.Id
            };

            var notification = new Notification
            {
                ChannelId = channelId,
                Recipient = user.Email!,
                UserId = user.Id,
                Payload = JsonSerializer.Serialize(emailPayload),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            return await _notificationRepository.CreateAsync(notification, cancellationToken);
        }

        private async Task<Notification> CreateLoginSmsNotificationAsync(
            ApplicationUser user,
            int channelId,
            CancellationToken cancellationToken)
        {
            var loginTime = DateTime.UtcNow;
            var templateParameters = new Dictionary<string, string>
            {
                { "firstName", user.FirstName },
                { "loginTime", loginTime.ToString("HH:mm") }
            };

            var message = await _templateService.GetSmsTemplateAsync("LoginNotification", templateParameters);

            var smsPayload = new
            {
                message = message,
                loginTime = loginTime,
                userId = user.Id
            };

            var notification = new Notification
            {
                ChannelId = channelId,
                Recipient = user.PhoneNumber!,
                UserId = user.Id,
                Payload = JsonSerializer.Serialize(smsPayload),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            return await _notificationRepository.CreateAsync(notification, cancellationToken);
        }

        private async Task<Notification> CreateWelcomeEmailNotificationAsync(
            ApplicationUser user,
            int channelId,
            CancellationToken cancellationToken)
        {
            var templateParameters = new Dictionary<string, string>
            {
                { "firstName", user.FirstName },
                { "lastName", user.LastName }
            };

            var htmlMessage = await _templateService.GetEmailTemplateAsync("WelcomeRegistration", templateParameters);

            var emailPayload = new
            {
                subject = "Welcome to SkyReserve!",
                htmlMessage = htmlMessage,
                registrationDate = DateTime.UtcNow,
                userId = user.Id
            };

            var notification = new Notification
            {
                ChannelId = channelId,
                Recipient = user.Email!,
                UserId = user.Id,
                Payload = JsonSerializer.Serialize(emailPayload),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            return await _notificationRepository.CreateAsync(notification, cancellationToken);
        }

        private async Task<Notification> CreateWelcomeSmsNotificationAsync(
            ApplicationUser user,
            int channelId,
            CancellationToken cancellationToken)
        {
            var templateParameters = new Dictionary<string, string>
            {
                { "firstName", user.FirstName }
            };

            var message = await _templateService.GetSmsTemplateAsync("WelcomeRegistration", templateParameters);

            var smsPayload = new
            {
                message = message,
                registrationDate = DateTime.UtcNow,
                userId = user.Id
            };

            var notification = new Notification
            {
                ChannelId = channelId,
                Recipient = user.PhoneNumber!,
                UserId = user.Id,
                Payload = JsonSerializer.Serialize(smsPayload),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            return await _notificationRepository.CreateAsync(notification, cancellationToken);
        }

        public async Task SendNotificationToSqsAsync(int notificationId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the notification to determine the message type
                var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
                if (notification == null)
                {
                    _logger.LogWarning("Notification not found: {NotificationId}", notificationId);
                    return;
                }

                string messageType = "BookingConfirmation";

                if (notification.Payload.Contains("Login Notification"))
                    messageType = "LoginNotification";
                else if (notification.Payload.Contains("Welcome to SkyReserve"))
                    messageType = "RegistrationWelcome";
                else if (notification.Payload.Contains("Email Confirmation"))
                    messageType = "EmailConfirmation";
                else if (notification.Payload.Contains("Password Reset"))
                    messageType = "PasswordReset";

                var messageBody = JsonSerializer.Serialize(new { notificationId });

                var sendMessageRequest = new SendMessageRequest
                {
                    QueueUrl = _sqsSettings.QueueUrl,
                    MessageBody = messageBody,
                    MessageAttributes = new Dictionary<string, MessageAttributeValue>
                    {
                        {
                            "NotificationId",
                            new MessageAttributeValue
                            {
                                DataType = "Number",
                                StringValue = notificationId.ToString()
                            }
                        },
                        {
                            "MessageType",
                            new MessageAttributeValue
                            {
                                DataType = "String",
                                StringValue = messageType
                            }
                        }
                    }
                };

                var response = await _sqsClient.SendMessageAsync(sendMessageRequest, cancellationToken);

                _logger.LogInformation("Sent notification {NotificationId} to SQS. MessageId: {MessageId}",
                    notificationId, response.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification {NotificationId} to SQS", notificationId);
                throw;
            }
        }

        private async Task<NotificationChannel> GetOrCreateEmailChannelAsync(CancellationToken cancellationToken)
        {
            var channel = await _notificationRepository.GetChannelByTypeAsync("EMAIL", cancellationToken);

            if (channel == null)
            {
                channel = new NotificationChannel
                {
                    ChannelType = "EMAIL",
                    ChannelName = "AWS SES",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                channel = await _notificationRepository.CreateChannelAsync(channel, cancellationToken);
            }

            return channel;
        }

        private async Task<NotificationChannel> GetOrCreateSmsChannelAsync(CancellationToken cancellationToken)
        {
            var channel = await _notificationRepository.GetChannelByTypeAsync("SMS", cancellationToken);

            if (channel == null)
            {
                channel = new NotificationChannel
                {
                    ChannelType = "SMS",
                    ChannelName = "Twilio",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                channel = await _notificationRepository.CreateChannelAsync(channel, cancellationToken);
            }

            return channel;
        }

        public async Task CreateEmailConfirmationNotificationAsync(string userId, string confirmationCode, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return;
                }

                var emailChannel = await GetOrCreateEmailChannelAsync(cancellationToken);

                if (!string.IsNullOrEmpty(user.Email))
                {
                    var emailNotification = await CreateEmailConfirmationEmailNotificationAsync(user, confirmationCode, emailChannel.ChannelId, cancellationToken);
                    await SendNotificationToSqsAsync(emailNotification.NotificationId, cancellationToken);
                }

                _logger.LogInformation("Created email confirmation notification for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating email confirmation notification for user {UserId}", userId);
                throw;
            }
        }

        private async Task<Notification> CreateEmailConfirmationEmailNotificationAsync(
            ApplicationUser user,
            string confirmationCode,
            int channelId,
            CancellationToken cancellationToken)
        {
            var templateParameters = new Dictionary<string, string>
            {
                { "firstName", user.FirstName },
                { "confirmationCode", confirmationCode },
                { "userId", user.Id }
            };

            var htmlMessage = await _templateService.GetEmailTemplateAsync("EmailConfirmation", templateParameters);

            var emailPayload = new
            {
                subject = "Email Confirmation - SkyReserve",
                htmlMessage = htmlMessage,
                confirmationCode = confirmationCode,
                userId = user.Id
            };

            var notification = new Notification
            {
                ChannelId = channelId,
                Recipient = user.Email!,
                UserId = user.Id,
                Payload = JsonSerializer.Serialize(emailPayload),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            return await _notificationRepository.CreateAsync(notification, cancellationToken);
        }

        public async Task CreatePasswordResetNotificationAsync(string userId, string resetCode, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return;
                }

                var emailChannel = await GetOrCreateEmailChannelAsync(cancellationToken);

                if (!string.IsNullOrEmpty(user.Email))
                {
                    var emailNotification = await CreatePasswordResetEmailNotificationAsync(user, resetCode, emailChannel.ChannelId, cancellationToken);
                    await SendNotificationToSqsAsync(emailNotification.NotificationId, cancellationToken);
                }

                _logger.LogInformation("Created password reset notification for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating password reset notification for user {UserId}", userId);
                throw;
            }
        }

        private async Task<Notification> CreatePasswordResetEmailNotificationAsync(
            ApplicationUser user,
            string resetCode,
            int channelId,
            CancellationToken cancellationToken)
        {
            var templateParameters = new Dictionary<string, string>
            {
                { "firstName", user.FirstName },
                { "resetCode", resetCode },
                { "userId", user.Id },
                { "email", user.Email! }
            };

            var htmlMessage = await _templateService.GetEmailTemplateAsync("ForgetPassword", templateParameters);

            var emailPayload = new
            {
                subject = "Password Reset - SkyReserve",
                htmlMessage = htmlMessage,
                resetCode = resetCode,
                userId = user.Id,
                email = user.Email
            };

            var notification = new Notification
            {
                ChannelId = channelId,
                Recipient = user.Email!,
                UserId = user.Id,
                Payload = JsonSerializer.Serialize(emailPayload),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            return await _notificationRepository.CreateAsync(notification, cancellationToken);
        }
    }
}