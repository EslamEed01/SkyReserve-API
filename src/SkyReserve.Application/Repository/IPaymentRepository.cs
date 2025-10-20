namespace SkyReserve.Application.Repository
{
    public interface IPaymentRepository
    {
        Task<SkyReserve.Domain.Entities.Payment> AddAsync(SkyReserve.Domain.Entities.Payment payment, CancellationToken cancellationToken);
        Task<SkyReserve.Domain.Entities.Payment?> GetByIdAsync(int paymentId, CancellationToken cancellationToken);
        Task<SkyReserve.Domain.Entities.Payment?> GetByIdAsync(string StripePaymentIntent, CancellationToken cancellationToken);
        Task<IEnumerable<SkyReserve.Domain.Entities.Payment>> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken);
        Task UpdateAsync(SkyReserve.Domain.Entities.Payment payment, CancellationToken cancellationToken);

    }
}
