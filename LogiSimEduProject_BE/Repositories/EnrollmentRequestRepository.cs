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
    public class EnrollmentRequestRepository : GenericRepository<AccountOfCourse>
    {
        public EnrollmentRequestRepository() { }

        public async Task<List<AccountOfCourse>> GetAll()
        {
            return await _context.AccountOfCourses
                .Include(e => e.Account)
                .Include(e => e.Course)
                .ToListAsync();
        }
        public async Task<List<AccountOfCourse>> GetByCourseId(string courseId)
        {
            return await _context.AccountOfCourses
                .Include(e => e.Account)
                .Where(e => e.CourseId == Guid.Parse(courseId))
                .ToListAsync();
        }
        public async Task<AccountOfCourse> GetById(string id)
        {
            return await _context.AccountOfCourses
                .Include(e => e.Account)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == Guid.Parse(id));
        }

        public async Task<AccountOfCourse?> GetAcceptedRequest(Guid studentId)
        {
            return await _context.AccountOfCourses
                .FirstOrDefaultAsync(x => x.AccountId == studentId && x.Status == 2);
        }
    }
}
