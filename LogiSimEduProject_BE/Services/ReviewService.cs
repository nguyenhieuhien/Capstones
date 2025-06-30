using Repositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IReviewService
    {
        Task<List<Review>> GetAll();
        Task<Review> GetById(string id);
        Task<int> Create(Review review);
        Task<int> Update(Review review);
        Task<bool> Delete(string id);
    }
    public class ReviewService : IReviewService
    {
        private readonly ReviewRepository _repository;

        public ReviewService()
        {
            _repository = new ReviewRepository();
        }

        public async Task<int> Create(Review review)
        {
            if (review == null || string.IsNullOrEmpty(review.Description) || review.Rating < 1 || review.Rating > 5)
            {
                return 0;
            }
            review.Id = Guid.NewGuid();
            var result = await _repository.CreateAsync(review);
            return result;

        }

        public async Task<bool> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }
            var item = await _repository.GetByIdAsync(id);
            if (item != null)
            {
                var result = await _repository.RemoveAsync(item);
                return result;
            }
            return false;
        }

        public async Task<List<Review>> GetAll()
        {
            var reviews = await _repository.GetAll();
            return reviews ?? new List<Review>();
        }

        public async Task<Review> GetById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            var review = await _repository.GetByIdAsync(id);
            return review;
        }

        public async Task<int> Update(Review review)
        {
            if (review == null || review.Id == Guid.Empty || string.IsNullOrEmpty(review.Description) || review.Rating < 1 || review.Rating > 5)
            {
                return 0;
            }
            var existingReview = await _repository.GetByIdAsync(review.Id);
            if (existingReview == null)
            {
                return 0; // Review not found
            }
            var result = await _repository.UpdateAsync(review);
            return result;
        }
    }
}

