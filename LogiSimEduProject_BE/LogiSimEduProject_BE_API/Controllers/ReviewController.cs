
using LogiSimEduProject_BE_API.Controllers.DTO.Review;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Controllers
{
    [ApiController]
    [Route("api/review")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController()
        {
            _reviewService = new ReviewService();
        }

        [HttpGet("get_all_review")]
        [SwaggerOperation(Summary = "Get all reviews", Description = "Returns a list of all reviews.")]
        public async Task<ActionResult<List<Review>>> GetAll()
        {
            var reviews = await _reviewService.GetAll();
            return Ok(reviews);
        }

        [HttpGet("get_review/{id}")]
        [SwaggerOperation(Summary = "Get a review by ID", Description = "Returns a specific review by its ID.")]
        public async Task<ActionResult<Review>> GetById(string id)
        {
            var review = await _reviewService.GetById(id);
            if (review == null)
                return NotFound();
            return Ok(review);
        }

        [HttpPost("create_review")]
        [SwaggerOperation(Summary = "Create a new review", Description = "Creates a new review for a course.")]
        public async Task<ActionResult<int>> Create([FromBody] ReviewCreateDTO reviewDto)
        {
            if (reviewDto == null)
                return BadRequest("Review data is null.");
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
                return BadRequest("Failed to create review.");
            return Ok(result);
        }

        [HttpPut("update_review/{id}")]
        [SwaggerOperation(Summary = "Update an existing review", Description = "Updates the description and rating of a review.")]
        public async Task<ActionResult<int>> Update(string id, [FromBody] ReviewUpdateDTO reviewDto)
        {
            if (reviewDto == null)
                return BadRequest("Review data is null.");
            var existingReview = await _reviewService.GetById(id);
            if (existingReview == null)
                return NotFound("Review not found.");
            existingReview.Description = reviewDto.Description;
            existingReview.Rating = reviewDto.Rating;
            existingReview.UpdatedAt = DateTime.UtcNow;
            var result = await _reviewService.Update(existingReview);
            if (result == 0)
                return BadRequest("Failed to update review.");
            return Ok(result);
        }

        [HttpDelete("delete_review/{id}")]
        [SwaggerOperation(Summary = "Delete a review", Description = "Deletes a review by its ID.")]
        public async Task<ActionResult<bool>> Delete(string id)
        {
            var result = await _reviewService.Delete(id);
            if (!result)
                return NotFound();
            return Ok(result);
        }
    }
}
