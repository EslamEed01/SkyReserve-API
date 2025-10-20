namespace SkyReserve.Domain.Entities
{
    public class Payment
    {

        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public string? StripePaymentIntentId { get; set; }

        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } 
        public string PaymentStatus { get; set; }
        public string TransactionId { get; set; }

        public string UserId { get; set; }

        // Navigation property
        public Booking Booking { get; set; }
        public ApplicationUser applicationUser { get; set; } = null!;
    }
}
