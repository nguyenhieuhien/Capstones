// File: Services/ReviewService.cs
using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.DBContext;
using Repositories.Models;
using Services.DTO.Review;
using Services.IServices;

namespace Services
{
    public class ReviewService : IReviewService
    {
        private readonly ReviewRepository _repository;
        private readonly CourseRepository _courseRepo;
        private readonly LogiSimEduContext _dbContext;

        public ReviewService(LogiSimEduContext dbContext)
        {
            _repository = new ReviewRepository();
            _courseRepo = new CourseRepository();
            _dbContext = dbContext;
        }

        public async Task<List<Review>> GetAll()
        {
            return await _repository.GetAll() ?? new List<Review>();
        }

        public async Task<Review?> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return await _repository.GetByIdAsync(id);
        }

        public async Task<List<Review>> GetReviewsByCourseIdAsync(Guid courseId)
        {
            return await _repository.GetReviewsByCourseIdAsync(courseId);
        }

        public async Task<(bool Success, string Message)> AddOrUpdateReview(Guid courseId, Guid accountId, string description, int rating)
        {
            if (rating < 1 || rating > 5)
                return (false, "Rating must be between 1 and 5");

            var course = await _courseRepo.GetCourseByIdAsync(courseId);
            if (course == null || course.IsActive == false)
                return (false, "Course not found or inactive");

            var existingReview = await _repository.GetReviewByStudentAsync(courseId, accountId);
            if (existingReview != null)
                return (false, "You have already reviewed this course. Use update instead.");

            var newReview = new Review
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                CourseId = courseId,
                Description = description,
                Rating = rating,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _repository.CreateAsync(newReview);
            // Update average rating of course
            course.RatingAverage = await _repository.GetAverageRatingAsync(courseId);
            await _courseRepo.UpdateAsync(course);

            return (true, "Review submitted successfully");
        }

        public async Task<int> Create(Review review)
        {
            if (review == null || string.IsNullOrWhiteSpace(review.Description) || review.Rating < 1 || review.Rating > 5)
                return 0;

            review.Id = Guid.NewGuid();
            review.IsActive = true;
            review.CreatedAt = DateTime.UtcNow;
            review.UpdatedAt = null;

            return await _repository.CreateAsync(review);
        }

        public async Task<(bool Success, string Message)> UpdateReview(
    Guid courseId,
    Guid accountId,
    string description,
    int rating)
        {
            if (rating < 1 || rating > 5)
                return (false, "Rating must be between 1 and 5");

            var course = await _courseRepo.GetCourseByIdAsync(courseId);
            if (course == null || course.IsActive == false)
                return (false, "Course not found or inactive.");

            var existingReview = await _repository.GetReviewByStudentAsync(courseId, accountId);
            if (existingReview == null)
                return (false, "Review not found. Create first before updating.");

            existingReview.Description = description;
            existingReview.Rating = rating;
            existingReview.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(existingReview);

            // tính lại rating average
            course.RatingAverage = await _repository.GetAverageRatingAsync(courseId);
            await _courseRepo.UpdateAsync(course);

            return (true, "Review updated successfully.");
        }


        public async Task<int> Update(Review review)
        {
            if (review == null || review.Id == Guid.Empty || string.IsNullOrWhiteSpace(review.Description) || review.Rating < 1 || review.Rating > 5)
                return 0;

            var existing = await _repository.GetByIdAsync(review.Id);
            if (existing == null) return 0;

            review.UpdatedAt = DateTime.UtcNow;
            return await _repository.UpdateAsync(review);
        }

        public async Task<bool> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;

            var review = await _repository.GetByIdAsync(id);
            if (review == null) return false;

            return await _repository.RemoveAsync(review);
        }
    }
}
