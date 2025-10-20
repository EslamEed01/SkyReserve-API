using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyReserve.Application.DTOs.Review.DTOs;
using SkyReserve.Application.Interfaces;
using SkyReserve.Infrastructure.Authorization;
using System.Security.Claims;

namespace SkyReserve.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Create a new review
        /// </summary>
        [HttpPost]
        [HasPermission(Permissions.Reviews.Create)]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");

                request.UserId = userId;

                var result = await _reviewService.CreateReviewAsync(request);
                return CreatedAtAction(nameof(GetReview), new { id = result.ReviewId }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get a review by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReview(int id)
        {
            var result = await _reviewService.GetReviewAsync(id);
            if (result == null)
                return NotFound($"Review with ID {id} not found");

            return Ok(result);
        }

        /// <summary>
        /// Update a review
        /// </summary>
        [HttpPut("{id}")]
        [HasPermission(Permissions.Reviews.UpdateOwn)]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewDto request)
        {
            try
            {
                if (id != request.ReviewId)
                    return BadRequest("Review ID mismatch");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var existingReview = await _reviewService.GetReviewAsync(id);

                if (existingReview == null)
                    return NotFound($"Review with ID {id} not found");

                if (existingReview.UserId != userId && !User.IsInRole("Admin"))
                    return Forbid("You can only update your own reviews");

                var result = await _reviewService.UpdateReviewAsync(request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Delete a review
        /// </summary>
        [HttpDelete("{id}")]
        [HasPermission(Permissions.Reviews.DeleteOwn)]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = User.IsInRole("Admin");

                var result = await _reviewService.DeleteReviewAsync(id, userId ?? "", isAdmin);
                if (!result)
                    return NotFound($"Review with ID {id} not found");

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        /// <summary>
        /// Get all reviews with pagination
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllReviews([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _reviewService.GetAllReviewsAsync(pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Get reviews by flight ID
        /// </summary>
        [HttpGet("flight/{flightId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFlightReviews(int flightId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _reviewService.GetFlightReviewsAsync(flightId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get current user's reviews
        /// </summary>
        [HttpGet("my-reviews")]
        [HasPermission(Permissions.Reviews.ViewOwn)]
        public async Task<IActionResult> GetMyReviews([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            var result = await _reviewService.GetUserReviewsAsync(userId, pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Get average rating for a flight
        /// </summary>
        [HttpGet("flight/{flightId}/average-rating")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFlightAverageRating(int flightId)
        {
            var result = await _reviewService.GetFlightAverageRatingAsync(flightId);
            return Ok(new { flightId, averageRating = result });
        }


    }
}