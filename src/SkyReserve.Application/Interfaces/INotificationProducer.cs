namespace SkyReserve.Application.Interfaces
{
    public interface INotificationProducer
    {
        Task CreateBookingConfirmationNotificationAsync(int bookingId, string userId, CancellationToken cancellationToken = default);
        Task CreateLoginNotificationAsync(string userId, CancellationToken cancellationToken = default);
        Task CreateRegistrationWelcomeNotificationAsync(string userId, CancellationToken cancellationToken = default);
        Task CreateEmailConfirmationNotificationAsync(string userId, string confirmationCode, CancellationToken cancellationToken = default);
        Task CreatePasswordResetNotificationAsync(string userId, string resetCode, CancellationToken cancellationToken = default);
        Task SendNotificationToSqsAsync(int notificationId, CancellationToken cancellationToken = default);
    }
}
