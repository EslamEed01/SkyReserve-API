using Microsoft.Extensions.Configuration;
using SkyReserve.Application.DTOs.Payment.DTOs;
using SkyReserve.Application.Interfaces;
using Stripe;


namespace SkyReserve.Infrastructure.Services
{
    public class StripePaymentService : IStripePayment
    {
        public StripePaymentService(IConfiguration configuration)
        {
            StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
        }

        public async Task<Stripe.PaymentIntent> CreatePaymentIntentAsync(long amount, string currency)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = currency,
                PaymentMethodTypes = new List<string> { "card" },
                //AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                //{
                //    Enabled = true
                //}
            };

            var service = new PaymentIntentService();
            return await service.CreateAsync(options);
        }

        public async Task<PaymentIntentResponse> CreatePaymentIntentForOrderAsync(CreatePaymentIntentRequest request)
        {
            var amountInCents = (long)(request.Amount * 100);
            var paymentIntent = await CreatePaymentIntentAsync(amountInCents, request.Currency);

            return new PaymentIntentResponse
            {
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret,
                Amount = request.Amount,
                Currency = request.Currency,
                Status = paymentIntent.Status
            };
        }
    }
}