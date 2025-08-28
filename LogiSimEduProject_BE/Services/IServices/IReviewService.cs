using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface IReviewService
    {
        Task<List<Review>> GetAll();
        Task<Review?> GetById(string id);
        Task<List<Review>> GetReviewsByCourseIdAsync(Guid courseId);
        Task<(bool Success, string Message)> AddOrUpdateReview(Guid courseId, Guid accountId, string description, int rating);
        Task<(bool Success, string Message)> UpdateReview(Guid courseId, Guid accountId, string description, int rating);
        Task<int> Create(Review review);
        Task<int> Update(Review review);
        Task<bool> Delete(string id);
    }
}
