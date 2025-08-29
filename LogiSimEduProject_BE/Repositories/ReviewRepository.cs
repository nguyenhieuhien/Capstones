using Microsoft.EntityFrameworkCore;
using Repositories.Base;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class ReviewRepository : GenericRepository<Review>
    {
        public ReviewRepository() { }

        public new async Task<List<Review>> GetAll()
        {
            var reviews = await _context.Reviews.ToListAsync();
            if (reviews == null || !reviews.Any())
            {
                throw new InvalidOperationException("No reviews found.");
            }
            return reviews;
        }

        public async Task<List<Review>> GetReviewsByCourseIdAsync(Guid courseId)
        {
            return await _context.Reviews
                .Include(a => a.Account)
                .Include(c => c.Course)
                .Where(r => r.CourseId == courseId && r.IsActive == true)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review?> GetReviewByStudentAsync(Guid courseId, Guid accountId)
        {
            return await _context.Reviews
                .Include(r => r.Account)
                .Include(r => r.Course)
                .FirstOrDefaultAsync(r => r.CourseId == courseId && r.AccountId == accountId && r.IsActive == true);
        }

        public async Task<double> GetAverageRatingAsync(Guid courseId)
        {
            var reviews = await _context.Reviews
       .Where(r => r.CourseId == courseId && r.IsActive == true && r.Rating.HasValue)
       .ToListAsync();

            return reviews.Any() ? reviews.Average(r => r.Rating.Value) : 0;
        }
    }
}
