using Microsoft.EntityFrameworkCore;
using SkyReserve.Application.Repository;
using SkyReserve.Infrastructure.Persistence;

namespace SkyReserve.Infrastructure.Repository.implementation
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly SkyReserveDbContext _context;

        public PaymentRepository(SkyReserveDbContext context)
        {
            _context = context;
        }

        public async Task<SkyReserve.Domain.Entities.Payment> AddAsync(SkyReserve.Domain.Entities.Payment payment, CancellationToken cancellationToken = default)
        {
            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            payment.PaymentDate = DateTime.UtcNow;

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync(cancellationToken);

            return payment;
        }

        public async Task<SkyReserve.Domain.Entities.Payment?> GetByIdAsync(int paymentId, CancellationToken cancellationToken = default)
        {
            return await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.User)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Flight)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId, cancellationToken);
        }

        public async Task<SkyReserve.Domain.Entities.Payment?> GetByIdAsync(string stripePaymentIntent, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(stripePaymentIntent))
                throw new ArgumentException("Stripe payment intent cannot be null or empty", nameof(stripePaymentIntent));

            return await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.User)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Flight)
                .FirstOrDefaultAsync(p => p.TransactionId == stripePaymentIntent, cancellationToken);
        }

        public async Task<IEnumerable<SkyReserve.Domain.Entities.Payment>> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
        {
           
            return await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.User)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Flight)
                .Where(p => p.Booking.UserId == customerId.ToString())
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateAsync(SkyReserve.Domain.Entities.Payment payment, CancellationToken cancellationToken = default)
        {
            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.PaymentId == payment.PaymentId, cancellationToken);

            if (existingPayment == null)
                throw new KeyNotFoundException($"Payment with ID {payment.PaymentId} not found");

            existingPayment.Amount = payment.Amount;
            existingPayment.PaymentMethod = payment.PaymentMethod;
            existingPayment.PaymentStatus = payment.PaymentStatus;
            existingPayment.TransactionId = payment.TransactionId;
            existingPayment.PaymentDate = payment.PaymentDate;

            _context.Payments.Update(existingPayment);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
