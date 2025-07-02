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
    public class EnrollmentRequestRepository : GenericRepository<EnrollmentRequest>
    {
        public EnrollmentRequestRepository() { }

        public async Task<List<EnrollmentRequest>> GetAll()
        {
            return await _context.EnrollmentRequests
                .Include(e => e.Student)
                .Include(e => e.Course)
                .ToListAsync();
        }
        public async Task<List<EnrollmentRequest>> GetByCourseId(string courseId)
        {
            return await _context.EnrollmentRequests
                .Include(e => e.Student)
                .Where(e => e.CourseId == Guid.Parse(courseId))
                .ToListAsync();
        }
        public async Task<EnrollmentRequest> GetById(string id)
        {
            return await _context.EnrollmentRequests
                .Include(e => e.Student)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == Guid.Parse(id));
        }

        public async Task<EnrollmentRequest?> GetAcceptedRequest(Guid studentId)
        {
            return await _context.EnrollmentRequests
                .FirstOrDefaultAsync(x => x.StudentId == studentId && x.Status == "Accepted");
        }
    }
}
