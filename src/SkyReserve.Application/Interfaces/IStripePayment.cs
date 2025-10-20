using SkyReserve.Application.DTOs.Payment.DTOs;

namespace SkyReserve.Application.Interfaces
{
    public interface IStripePayment
    {
        Task<Stripe.PaymentIntent> CreatePaymentIntentAsync(long amount, string currency);
        Task<PaymentIntentResponse> CreatePaymentIntentForOrderAsync(CreatePaymentIntentRequest request);
    }
}
