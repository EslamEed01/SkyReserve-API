using Microsoft.EntityFrameworkCore;
using SkyReserve.Application.DTOs.Review.DTOs;
using SkyReserve.Application.Repository;
using SkyReserve.Infrastructure.Persistence;

namespace SkyReserve.Infrastructure.Repository.implementation
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly SkyReserveDbContext _context;

        public ReviewRepository(SkyReserveDbContext context)
        {
            _context = context;
        }

        public async Task<ReviewDto> CreateAsync(CreateReviewDto review)
        {
            var reviewEntity = new SkyReserve.Domain.Entities.Review
            {
                UserId = review.UserId,
                FlightId = review.FlightId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(reviewEntity);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(reviewEntity.ReviewId) ?? throw new InvalidOperationException("Failed to create review");
        }

        public async Task<ReviewDto> UpdateAsync(UpdateReviewDto review)
        {
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewId == review.ReviewId);

            if (existingReview == null)
                throw new KeyNotFoundException($"Review with ID {review.ReviewId} not found");

            existingReview.Rating = review.Rating;
            existingReview.Comment = review.Comment;
            existingReview.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(review.ReviewId) ?? throw new InvalidOperationException("Failed to update review");
        }

        public async Task<bool> DeleteAsync(int reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
                return false;

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ReviewDto?> GetByIdAsync(int reviewId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Flight)
                    .ThenInclude(f => f.DepartureAirport)
                .Include(r => r.Flight)
                    .ThenInclude(f => f.ArrivalAirport)
                .Where(r => r.ReviewId == reviewId)
                .Select(r => new ReviewDto
                {
                    ReviewId = r.ReviewId,
                    UserId = r.UserId,
                    FlightId = r.FlightId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    UserName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : "Unknown User",
                    FlightNumber = r.Flight.FlightNumber,
                    DepartureCity = r.Flight.DepartureAirport.City,
                    ArrivalCity = r.Flight.ArrivalAirport.City
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ReviewDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Flight)
                    .ThenInclude(f => f.DepartureAirport)
                .Include(r => r.Flight)
                    .ThenInclude(f => f.ArrivalAirport)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReviewDto
                {
                    ReviewId = r.ReviewId,
                    UserId = r.UserId,
                    FlightId = r.FlightId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    UserName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : "Unknown User",
                    FlightNumber = r.Flight.FlightNumber,
                    DepartureCity = r.Flight.DepartureAirport.City,
                    ArrivalCity = r.Flight.ArrivalAirport.City
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ReviewDto>> GetByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 10)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Flight)
                    .ThenInclude(f => f.DepartureAirport)
                .Include(r => r.Flight)
                    .ThenInclude(f => f.ArrivalAirport)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReviewDto
                {
                    ReviewId = r.ReviewId,
                    UserId = r.UserId,
                    FlightId = r.FlightId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    UserName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : "Unknown User",
                    FlightNumber = r.Flight.FlightNumber,
                    DepartureCity = r.Flight.DepartureAirport.City,
                    ArrivalCity = r.Flight.ArrivalAirport.City
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ReviewDto>> GetByFlightIdAsync(int flightId, int pageNumber = 1, int pageSize = 10)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Flight)
                    .ThenInclude(f => f.DepartureAirport)
                .Include(r => r.Flight)
                    .ThenInclude(f => f.ArrivalAirport)
                .Where(r => r.FlightId == flightId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReviewDto
                {
                    ReviewId = r.ReviewId,
                    UserId = r.UserId,
                    FlightId = r.FlightId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    UserName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : "Unknown User",
                    FlightNumber = r.Flight.FlightNumber,
                    DepartureCity = r.Flight.DepartureAirport.City,
                    ArrivalCity = r.Flight.ArrivalAirport.City
                })
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int reviewId)
        {
            return await _context.Reviews.AnyAsync(r => r.ReviewId == reviewId);
        }



        public async Task<double> GetAverageRatingForFlightAsync(int flightId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.FlightId == flightId)
                .ToListAsync();

            return reviews.Any() ? reviews.Average(r => r.Rating) : 0.0;
        }


    }
}