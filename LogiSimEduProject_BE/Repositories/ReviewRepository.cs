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
    }
}
