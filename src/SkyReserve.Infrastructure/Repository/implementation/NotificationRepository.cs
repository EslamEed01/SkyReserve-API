using Microsoft.EntityFrameworkCore;
using SkyReserve.Application.Repository;
using SkyReserve.Domain.Entities;
using SkyReserve.Infrastructure.Persistence;

namespace SkyReserve.Infrastructure.Repository.implementation
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly SkyReserveDbContext _context;

        public NotificationRepository(SkyReserveDbContext context)
        {
            _context = context;
        }

        public async Task<Notification?> GetByIdAsync(int notificationId, CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .Include(n => n.Channel)
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId, cancellationToken);
        }

     

        public async Task UpdateStatusAsync(int notificationId, string status, DateTime? sentAt = null, CancellationToken cancellationToken = default)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId, cancellationToken);

            if (notification != null)
            {
                notification.Status = status;
                if (sentAt.HasValue)
                {
                    notification.SentAt = sentAt.Value;
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
        }

      
        public async Task<Notification> CreateAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(cancellationToken);
            return notification;
        }

        public async Task<NotificationChannel?> GetChannelByTypeAsync(string channelType, CancellationToken cancellationToken = default)
        {
            return await _context.NotificationChannels
                .FirstOrDefaultAsync(c => c.ChannelType == channelType && c.IsActive, cancellationToken);
        }

        public async Task<NotificationChannel> CreateChannelAsync(NotificationChannel channel, CancellationToken cancellationToken = default)
        {
            _context.NotificationChannels.Add(channel);
            await _context.SaveChangesAsync(cancellationToken);
            return channel;
        }
    }
}