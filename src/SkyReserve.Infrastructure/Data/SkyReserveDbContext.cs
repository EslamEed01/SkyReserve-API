using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SkyReserve.Domain.Entities;

namespace SkyReserve.Infrastructure.Persistence
{
    public class SkyReserveDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public SkyReserveDbContext(DbContextOptions<SkyReserveDbContext> options)
           : base(options) { }

        // DbSets
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Airport> Airports { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Price> Prices { get; set; }
        public DbSet<NotificationChannel> NotificationChannels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Airport
            modelBuilder.Entity<Airport>(entity =>
            {
                entity.HasKey(e => e.AirportId);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.City).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
            });

            // Flight
            modelBuilder.Entity<Flight>(entity =>
            {
                entity.HasKey(e => e.FlightId);
                entity.Property(e => e.FlightNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.AircraftModel).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.HasOne(f => f.DepartureAirport)
                    .WithMany(a => a.DepartingFlights)
                    .HasForeignKey(f => f.DepartureAirportId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(f => f.ArrivalAirport)
                    .WithMany(a => a.ArrivingFlights)
                    .HasForeignKey(f => f.ArrivalAirportId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Price
            modelBuilder.Entity<Price>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FareClass).IsRequired().HasMaxLength(50);
                entity.Property(e => e.BasePrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);

                entity.HasOne(p => p.Flight)
                    .WithMany(f => f.Prices)
                    .HasForeignKey(p => p.FlightId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.FlightId, e.FareClass, e.ValidFrom, e.ValidTo });
            });

            modelBuilder.Entity<Seat>(entity =>
            {
                entity.HasKey(e => e.SeatId);
                entity.Property(e => e.SeatNumber).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Class).HasMaxLength(50);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");

                entity.HasOne(s => s.Flight)
                    .WithMany(f => f.Seats)
                    .HasForeignKey(s => s.FlightId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.BookingId);
                entity.Property(e => e.BookingRef).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.FareClass).HasMaxLength(50);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.UserId).IsRequired(false);

                entity.HasIndex(e => e.BookingRef).IsUnique();

                entity.HasOne(b => b.User)
                    .WithMany(u => u.Bookings)
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(b => b.Flight)
                    .WithMany(f => f.Bookings)
                    .HasForeignKey(b => b.FlightId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Add relationship to Price
                entity.HasOne(b => b.Price)
                    .WithMany(p => p.Bookings)
                    .HasForeignKey(b => b.PriceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Passenger 
            modelBuilder.Entity<Passenger>(entity =>
            {
                entity.HasKey(e => e.PassengerId);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PassportNumber).HasMaxLength(50);
                entity.Property(e => e.Nationality).HasMaxLength(100);
                entity.Property(e => e.UserId).IsRequired(false);

                entity.HasOne(p => p.Booking)
                    .WithMany(b => b.Passengers)
                    .HasForeignKey(p => p.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Optional relationship with User
                entity.HasOne(p => p.User)
                    .WithMany(u => u.Passengers)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Payment
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.PaymentId);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.PaymentStatus).HasMaxLength(50);
                entity.Property(e => e.TransactionId).HasMaxLength(255);

                entity.HasOne(p => p.Booking)
                    .WithOne(b => b.Payment)
                    .HasForeignKey<Payment>(p => p.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Review 
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.ReviewId);
                entity.Property(e => e.Rating).IsRequired();
                entity.Property(e => e.Comment).HasMaxLength(1000);

                entity.HasOne(r => r.User)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Flight)
                    .WithMany(f => f.Reviews)
                    .HasForeignKey(r => r.FlightId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // NotificationChannel
            modelBuilder.Entity<NotificationChannel>(entity =>
            {
                entity.HasKey(e => e.ChannelId);
                entity.Property(e => e.ChannelType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ChannelName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Configuration).HasMaxLength(255);
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();

                entity.HasIndex(e => new { e.ChannelType, e.IsActive });
            });

            // Notification
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.NotificationId);
                entity.Property(e => e.Recipient).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Payload).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.RetryCount).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.SentAt).IsRequired(false);
                entity.Property(e => e.TemplateId).IsRequired(false);
                entity.Property(e => e.UserId).IsRequired(false);

                entity.HasOne(n => n.Channel)
                    .WithMany(c => c.Notifications)
                    .HasForeignKey(n => n.ChannelId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(n => n.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => new { e.ChannelId, e.Status });
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
