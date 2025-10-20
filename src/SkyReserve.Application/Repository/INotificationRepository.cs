using SkyReserve.Domain.Entities;

namespace SkyReserve.Application.Repository
{
    public interface INotificationRepository
    {
        Task<Notification?> GetByIdAsync(int notificationId, CancellationToken cancellationToken = default);
        Task UpdateStatusAsync(int notificationId, string status, DateTime? sentAt = null, CancellationToken cancellationToken = default);
        Task<Notification> CreateAsync(Notification notification, CancellationToken cancellationToken = default);
        Task<NotificationChannel?> GetChannelByTypeAsync(string channelType, CancellationToken cancellationToken = default);
        Task<NotificationChannel> CreateChannelAsync(NotificationChannel channel, CancellationToken cancellationToken = default);
    }
}