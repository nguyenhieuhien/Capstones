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
    public class CourseProgressRepository : GenericRepository<CourseProgress>
    {
        public CourseProgressRepository() { }
        public async Task<List<CourseProgress>> GetAll()
        {
            var courseProgresses = await _context.CourseProgresses.Where(a => a.IsActive == true).ToListAsync();

            return courseProgresses;
        }
        public async Task<CourseProgress?> GetByAccAndCourse(Guid accountId, Guid courseId)
        {
            return await _context.CourseProgresses
                .FirstOrDefaultAsync(cp => cp.AccountId == accountId && cp.CourseId == courseId && cp.IsActive == true);
        }
    }
}
