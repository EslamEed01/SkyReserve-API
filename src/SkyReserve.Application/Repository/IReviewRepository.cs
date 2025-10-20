using SkyReserve.Application.DTOs.Review.DTOs;

namespace SkyReserve.Application.Repository
{
    public interface IReviewRepository
    {
        Task<ReviewDto> CreateAsync(CreateReviewDto review);
        Task<ReviewDto> UpdateAsync(UpdateReviewDto review);
        Task<bool> DeleteAsync(int reviewId);
        Task<ReviewDto?> GetByIdAsync(int reviewId);
        Task<IEnumerable<ReviewDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<IEnumerable<ReviewDto>> GetByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 10);
        Task<IEnumerable<ReviewDto>> GetByFlightIdAsync(int flightId, int pageNumber = 1, int pageSize = 10);
        Task<bool> ExistsAsync(int reviewId);
        Task<double> GetAverageRatingForFlightAsync(int flightId);
    }
}