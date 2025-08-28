// File: Controllers/ReviewController.cs

using Google.Apis.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Services.DTO.Review;
using Services.IServices;
using Swashbuckle.AspNetCore.Annotations;

namespace LogiSimEduProject_BE_API.Controllers
{
    [ApiController]
    [Route("api/review")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        //[Authorize(Roles = "Instructor,Student")]
        [HttpGet("get_all_review")]
        [SwaggerOperation(Summary = "Get all reviews", Description = "Returns a list of all reviews.")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _reviewService.GetAll();
            return Ok(result);
        }

        //[Authorize(Roles = "Instructor,Student")]
        [HttpGet("get_review/{id}")]
        [SwaggerOperation(Summary = "Get a review by ID", Description = "Returns a specific review by its ID.")]
        public async Task<IActionResult> GetById(string id)
        {
            var review = await _reviewService.GetById(id);
            if (review == null)
                return NotFound(new { Message = "Review not found." });
            return Ok(review);
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetByCourseId(Guid courseId)
        {
            var result = await _reviewService.GetReviewsByCourseIdAsync(courseId);
            return Ok(result);
        }


        [HttpPost("{courseId}/review")]
        public async Task<IActionResult> AddOrUpdateReview(Guid courseId, Guid accountId, [FromBody] ReviewDTO dto)
        {
            var result = await _reviewService.AddOrUpdateReview(courseId, accountId, dto.Description, dto.Rating);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Message);
        }

        [HttpPut("{courseId}/update_reviews")]
        public async Task<IActionResult> UpdateReview(
    Guid courseId,
    Guid accountId,
    [FromBody] ReviewDTO dto)
        {
            var result = await _reviewService.UpdateReview(courseId, accountId, dto.Description, dto.Rating);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        //[Authorize(Roles = "Instructor,Student")]
        [HttpPost("create_review")]
        [SwaggerOperation(Summary = "Create a new review", Description = "Creates a new review for a course.")]
        public async Task<IActionResult> Create([FromBody] ReviewCreateDTO reviewDto)
        {
            if (reviewDto == null)
                return BadRequest(new { Message = "Review data is null." });

            var review = new Review
            {
                AccountId = reviewDto.AccountId,
                CourseId = reviewDto.CourseId,
                Description = reviewDto.Description,
                Rating = reviewDto.Rating,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _reviewService.Create(review);
            if (result == 0)
                return BadRequest(new { Message = "Failed to create review." });

            return Ok(new { Message = "Review created successfully.", ReviewId = review.Id });
        }

        //[Authorize(Roles = "Instructor,Student")]
        [HttpPut("update_review/{id}")]
        [SwaggerOperation(Summary = "Update an existing review", Description = "Updates the description and rating of a review.")]
        public async Task<IActionResult> Update(string id, [FromBody] ReviewUpdateDTO reviewDto)
        {
            if (reviewDto == null)
                return BadRequest(new { Message = "Review data is null." });

            var existingReview = await _reviewService.GetById(id);
            if (existingReview == null)
                return NotFound(new { Message = "Review not found." });

            existingReview.Description = reviewDto.Description;
            existingReview.Rating = reviewDto.Rating;
            existingReview.UpdatedAt = DateTime.UtcNow;

            var result = await _reviewService.Update(existingReview);
            if (result == 0)
                return BadRequest(new { Message = "Failed to update review." });

            return Ok(new { Message = "Review updated successfully." });
        }

        //[Authorize(Roles = "Instructor,Student")]
        [HttpDelete("delete_review/{id}")]
        [SwaggerOperation(Summary = "Delete a review", Description = "Deletes a review by its ID.")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _reviewService.Delete(id);
            if (!result)
                return NotFound(new { Message = "Review not found or already deleted." });
            return Ok(new { Message = "Review deleted successfully." });
        }
    }
}
