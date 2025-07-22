// File: Services/ReviewService.cs
using Repositories;
using Repositories.Models;
using Services.IServices;

namespace Services
{
    public class ReviewService : IReviewService
    {
        private readonly ReviewRepository _repository;

        public ReviewService()
        {
            _repository = new ReviewRepository();
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
