using SkyReserve.Domain.Entities;

namespace SkyReserve.Application.Interfaces
{
    public interface INotificationSender
    {

        Task SendAsync(Notification notification);

    }
}
