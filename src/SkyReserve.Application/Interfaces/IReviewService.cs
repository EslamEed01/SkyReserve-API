using SkyReserve.Application.DTOs.Review.DTOs;

namespace SkyReserve.Application.Interfaces
{
    public interface IReviewService
    {
        Task<ReviewDto> CreateReviewAsync(CreateReviewDto review);
        Task<ReviewDto> UpdateReviewAsync(UpdateReviewDto review);
        Task<bool> DeleteReviewAsync(int reviewId, string userId, bool isAdmin = false);
        Task<ReviewDto?> GetReviewAsync(int reviewId);
        Task<IEnumerable<ReviewDto>> GetAllReviewsAsync(int pageNumber = 1, int pageSize = 10);
        Task<IEnumerable<ReviewDto>> GetUserReviewsAsync(string userId, int pageNumber = 1, int pageSize = 10);
        Task<IEnumerable<ReviewDto>> GetFlightReviewsAsync(int flightId, int pageNumber = 1, int pageSize = 10);
        Task<double> GetFlightAverageRatingAsync(int flightId);
    }
}