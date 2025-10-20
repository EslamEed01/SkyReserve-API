using Microsoft.Extensions.DependencyInjection;
using SkyReserve.Application.Interfaces;
using SkyReserve.Domain.Entities;

namespace SkyReserve.Application.Services
{
    public static class NotificationFactory
    {
        public static INotificationSender GetSender(Notification notification, IServiceProvider serviceProvider)
        {
            if (notification.Channel == null)
            {
                throw new InvalidOperationException("Notification channel is required.");
            }

            return notification.Channel.ChannelType.ToUpperInvariant() switch
            {
                "EMAIL" => serviceProvider.GetRequiredService<EmailNotificationSender>(),
                "SMS" => serviceProvider.GetRequiredService<SmsNotificationSender>(),
                _ => throw new NotSupportedException($"Channel type '{notification.Channel.ChannelType}' is not supported.")
            };
        }

        public static INotificationSender GetSender(string channelType, IServiceProvider serviceProvider)
        {
            if (string.IsNullOrWhiteSpace(channelType))
            {
                throw new ArgumentException("Channel type cannot be null or empty.", nameof(channelType));
            }

            return channelType.ToUpperInvariant() switch
            {
                "EMAIL" => serviceProvider.GetRequiredService<EmailNotificationSender>(),
                "SMS" => serviceProvider.GetRequiredService<SmsNotificationSender>(),
                _ => throw new NotSupportedException($"Channel type '{channelType}' is not supported.")
            };
        }

        public static async Task SendNotificationAsync(Notification notification, IServiceProvider serviceProvider)
        {
            var sender = GetSender(notification, serviceProvider);
            await sender.SendAsync(notification);
        }
    }
}