using SkyReserve.Application.DTOs.Review.DTOs;
using SkyReserve.Application.Interfaces;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly IBookingRepository _bookingRepository;

        public ReviewService(
            IReviewRepository reviewRepository,
            IFlightRepository flightRepository,
            IBookingRepository bookingRepository)
        {
            _reviewRepository = reviewRepository;
            _flightRepository = flightRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<ReviewDto> CreateReviewAsync(CreateReviewDto review)
        {
            var flight = await _flightRepository.GetByIdAsync(review.FlightId);
            if (flight == null)
                throw new ArgumentException($"Flight with ID {review.FlightId} not found");


            return await _reviewRepository.CreateAsync(review);
        }

        public async Task<ReviewDto> UpdateReviewAsync(UpdateReviewDto review)
        {
            var existingReview = await _reviewRepository.GetByIdAsync(review.ReviewId);
            if (existingReview == null)
                throw new KeyNotFoundException($"Review with ID {review.ReviewId} not found");

            return await _reviewRepository.UpdateAsync(review);
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, string userId, bool isAdmin = false)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
                return false;

            if (!isAdmin && review.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own reviews");

            return await _reviewRepository.DeleteAsync(reviewId);
        }

        public async Task<ReviewDto?> GetReviewAsync(int reviewId)
        {
            return await _reviewRepository.GetByIdAsync(reviewId);
        }

        public async Task<IEnumerable<ReviewDto>> GetAllReviewsAsync(int pageNumber = 1, int pageSize = 10)
        {
            return await _reviewRepository.GetAllAsync(pageNumber, pageSize);
        }

        public async Task<IEnumerable<ReviewDto>> GetUserReviewsAsync(string userId, int pageNumber = 1, int pageSize = 10)
        {
            return await _reviewRepository.GetByUserIdAsync(userId, pageNumber, pageSize);
        }

        public async Task<IEnumerable<ReviewDto>> GetFlightReviewsAsync(int flightId, int pageNumber = 1, int pageSize = 10)
        {
            var flight = await _flightRepository.GetByIdAsync(flightId);
            if (flight == null)
                throw new ArgumentException($"Flight with ID {flightId} not found");

            return await _reviewRepository.GetByFlightIdAsync(flightId, pageNumber, pageSize);
        }

        public async Task<double> GetFlightAverageRatingAsync(int flightId)
        {
            return await _reviewRepository.GetAverageRatingForFlightAsync(flightId);
        }


    }
}