using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkyReserve.Application.Repository;
using SkyReserve.Application.Settings;
using System.Text.Json;

namespace SkyReserve.Application.Services
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationBackgroundService> _logger;
        private readonly SqsSettings _sqsSettings;
        private readonly IAmazonSQS _sqsClient;

        public NotificationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<NotificationBackgroundService> logger,
            IOptions<SqsSettings> sqsSettings,
            IAmazonSQS sqsClient)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _sqsSettings = sqsSettings.Value;
            _sqsClient = sqsClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificationBackgroundService started - listening for SQS messages");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessSqsMessagesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing SQS messages");
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }

        private async Task ProcessSqsMessagesAsync(CancellationToken cancellationToken)
        {
            try
            {
                var receiveRequest = new ReceiveMessageRequest
                {
                    QueueUrl = _sqsSettings.QueueUrl,
                    MaxNumberOfMessages = _sqsSettings.MaxMessages,
                    WaitTimeSeconds = _sqsSettings.WaitTimeSeconds,
                    VisibilityTimeout = _sqsSettings.VisibilityTimeoutSeconds
                };

                var response = await _sqsClient.ReceiveMessageAsync(receiveRequest, cancellationToken);

                if (response.Messages == null || response.Messages.Count == 0)
                {
                    return;
                }

                _logger.LogInformation("Received {MessageCount} messages from SQS", response.Messages.Count);

                foreach (var sqsMessage in response.Messages)
                {
                    await ProcessSingleSqsMessageAsync(sqsMessage, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving messages from SQS");
            }
        }

        private async Task ProcessSingleSqsMessageAsync(Message sqsMessage, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

            try
            {
                _logger.LogInformation("Processing SQS message: {MessageId}", sqsMessage.MessageId);

                var notificationId = ExtractNotificationIdFromMessage(sqsMessage.Body);

                if (!notificationId.HasValue)
                {
                    _logger.LogWarning("Could not extract notification ID from SQS message: {MessageId}", sqsMessage.MessageId);
                    await DeleteSqsMessageAsync(sqsMessage);
                    return;
                }

                var notification = await repository.GetByIdAsync(notificationId.Value, cancellationToken);

                if (notification == null)
                {
                    _logger.LogWarning("Notification {NotificationId} not found in database", notificationId.Value);
                    await DeleteSqsMessageAsync(sqsMessage);
                    return;
                }

                if (notification.Status != "Pending")
                {
                    _logger.LogInformation("Notification {NotificationId} is not pending (Status: {Status})",
                        notification.NotificationId, notification.Status);
                    await DeleteSqsMessageAsync(sqsMessage);
                    return;
                }

                await ProcessNotificationAsync(notification, repository, scope.ServiceProvider, cancellationToken);

                await DeleteSqsMessageAsync(sqsMessage);

                _logger.LogInformation("Successfully processed notification {NotificationId}", notification.NotificationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SQS message {MessageId}", sqsMessage.MessageId);

                await HandleFailedSqsMessageAsync(sqsMessage, repository, ex, cancellationToken);
            }
        }

        private async Task ProcessNotificationAsync(
            SkyReserve.Domain.Entities.Notification notification,
            INotificationRepository repository,
            IServiceProvider scopedServiceProvider,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing notification {NotificationId} for {Recipient} via {ChannelType}",
                    notification.NotificationId, notification.Recipient, notification.Channel?.ChannelType);

                await repository.UpdateStatusAsync(notification.NotificationId, "Processing", cancellationToken: cancellationToken);

                await NotificationFactory.SendNotificationAsync(notification, scopedServiceProvider); // Use scoped provider

                await repository.UpdateStatusAsync(notification.NotificationId, "Sent", DateTime.UtcNow, cancellationToken);

                _logger.LogInformation("Notification {NotificationId} sent successfully", notification.NotificationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification {NotificationId}", notification.NotificationId);

                await repository.UpdateStatusAsync(notification.NotificationId, "Failed", cancellationToken: cancellationToken);

                throw;
            }
        }

        private int? ExtractNotificationIdFromMessage(string messageBody)
        {
            try
            {
                var messageData = JsonSerializer.Deserialize<JsonElement>(messageBody);

                if (messageData.TryGetProperty("notificationId", out var notificationIdElement) &&
                    notificationIdElement.TryGetInt32(out var notificationId))
                {
                    return notificationId;
                }

                if (int.TryParse(messageBody.Trim(), out var directId))
                {
                    return directId;
                }

                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse SQS message body: {MessageBody}", messageBody);
                return null;
            }
        }

        private async Task DeleteSqsMessageAsync(Message message)
        {
            try
            {
                await _sqsClient.DeleteMessageAsync(_sqsSettings.QueueUrl, message.ReceiptHandle);
                _logger.LogDebug("Deleted SQS message: {MessageId}", message.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete SQS message: {MessageId}", message.MessageId);
            }
        }

        private async Task HandleFailedSqsMessageAsync(
            Message sqsMessage,
            INotificationRepository repository,
            Exception exception,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogError(exception, "Failed to process SQS message {MessageId}. Message will remain in queue for retry.",
                    sqsMessage.MessageId);

                var notificationId = ExtractNotificationIdFromMessage(sqsMessage.Body);
                if (notificationId.HasValue)
                {
                    await repository.UpdateStatusAsync(notificationId.Value, "Failed", cancellationToken: cancellationToken);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling failed SQS message {MessageId}", sqsMessage.MessageId);
            }
        }
    }
}