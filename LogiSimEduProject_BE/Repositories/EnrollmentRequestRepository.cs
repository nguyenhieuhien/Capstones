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
            return await _context.EnrollmentRequests.Where(a => a.IsActive == true)
                .Include(e => e.Account)
                .Include(e => e.Course)
                .ToListAsync();
        }
        public async Task<List<EnrollmentRequest>> GetByCourseId(string courseId)
        {
            return await _context.EnrollmentRequests
                .Include(e => e.Account)
                .Where(e => e.CourseId == Guid.Parse(courseId))
                .ToListAsync();
        }
        public async Task<EnrollmentRequest> GetById(string id)
        {
            return await _context.EnrollmentRequests
                .Include(e => e.Account)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == Guid.Parse(id));
        }

        public async Task<List<EnrollmentRequest>> GetStudentsByCourseId(Guid courseId)
        {
            return await _context.EnrollmentRequests
                .Where(aoc => aoc.CourseId == courseId && aoc.IsActive == true)
                .ToListAsync();
        }

        public async Task<EnrollmentRequest?> GetByAccountAndCourse(Guid accountOfCourseId)
        {
            return await _context.EnrollmentRequests
                .FirstOrDefaultAsync(x => x.Id == accountOfCourseId && x.Status == 1 && x.IsActive == true);
        }

        public async Task<EnrollmentRequest?> GetActiveByAccountAndCourseAsync(Guid accountId, Guid courseId)
        {
            return await _context.EnrollmentRequests
                .FirstOrDefaultAsync(x =>
                    x.AccountId == accountId &&
                    x.CourseId == courseId &&
                    x.IsActive == true);
        }

        public async Task<List<Course>> GetEnrolledCoursesByAccountId(Guid accountId)
        {
            return await _context.EnrollmentRequests
                .Where(aoc => aoc.AccountId == accountId && aoc.Status == 1) // 2 = Enrolled accepted
                .Include(aoc => aoc.Course)
                .Select(aoc => aoc.Course)
                .ToListAsync();
        }

        public async Task<List<Course>> GetPendingCoursesByAccountId(Guid accountId)
        {
            return await _context.EnrollmentRequests
                .Where(aoc => aoc.AccountId == accountId && aoc.Status == 0) // 2 = Enrolled accepted
                .Include(aoc => aoc.Course)
                .Select(aoc => aoc.Course)
                .ToListAsync();
        }

        public async Task<List<Account>> GetStudentsByClassId(Guid classId)
        {
            return await _context.EnrollmentRequests
                .Include(a => a.Account)
                .Where(a => a.ClassId == classId && a.Account.RoleId == 4 && a.IsActive == true)
                .Select(a => a.Account)
                .ToListAsync();
        }

        public async Task<int?> GetEnrollmentStatusAsync(Guid accountId, Guid courseId)
        {
            var enrollment = await _context.EnrollmentRequests
                .Where(aoc => aoc.AccountId == accountId && aoc.CourseId == courseId)
                .Select(aoc => aoc.Status)
                .FirstOrDefaultAsync();

            return enrollment; // null nếu không tìm thấy
        }

        public async Task<List<EnrollmentRequest>> GetEnrolledStudentsWithoutClass(Guid courseId)
        {
            return await _context.EnrollmentRequests
                .Include(aoc => aoc.Account)
                .Where(aoc => aoc.CourseId == courseId
                    && aoc.Status == 1
                    && aoc.ClassId == null
                    && aoc.Account.RoleId == 4
                    && aoc.IsActive == true)
                .ToListAsync();
        }

        public async Task<List<EnrollmentRequest>> GetPendingStudents(Guid courseId)
        {
            return await _context.EnrollmentRequests
                .Include(aoc => aoc.Account)
                .Where(aoc => aoc.CourseId == courseId
                    && aoc.Status == 0
                    && aoc.ClassId == null
                    && aoc.Account.RoleId == 4
                    && aoc.IsActive == true)
                .ToListAsync();
        }


        public async Task<EnrollmentRequest?> GetAcceptedRequest(Guid studentId)
        {
            return await _context.EnrollmentRequests
                .FirstOrDefaultAsync(x => x.AccountId == studentId && x.Status == 2);
        }
    }
}
