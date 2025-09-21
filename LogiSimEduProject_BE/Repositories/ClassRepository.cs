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
    public class ClassRepository : GenericRepository<Class>
    {
        public ClassRepository() { }

        public async Task<List<Class>> GetAll()
        {
            var classes = await _context.Classes.Where(a => a.IsActive == true).ToListAsync();

            return classes;
        }

        public async Task<List<Class>> GetByCourseAsync(Guid courseId)
        {
            var classes = await _context.Classes
                .Where(a => a.IsActive == true && a.CourseId == courseId)
                .ToListAsync();

            return classes;
        }

        public async Task<List<Class>> GetClassByAccountAsync(Guid accountId)
        {
            var classes = await _context.Classes
                .Where(a => a.IsActive == true && a.InstructorId == accountId)
                .ToListAsync();

            return classes;
        }

        public async Task<List<Class>> GetClassesByStudentIdAsync(Guid studentId)
        {
            return await _context.EnrollmentRequests
                .Include(a => a.Class)
                .ThenInclude(c => c.Course)
                .Where(a => a.AccountId == studentId && a.Class != null && a.IsActive == true)
                .Select(a => a.Class)
                .ToListAsync();
        }

        public async Task<Class?> GetClassByAccountAndCourseAsync(Guid accountId, Guid courseId)
        {
            var classEntity = await _context.EnrollmentRequests
                        .Where(aoc => aoc.IsActive == true
                      && aoc.AccountId == accountId
                      && aoc.CourseId == courseId
                      && aoc.Class != null)
                .Select(aoc => aoc.Class)
                .FirstOrDefaultAsync(); // chỉ lấy 1 cái

            return classEntity;
        }

       
    }
}
