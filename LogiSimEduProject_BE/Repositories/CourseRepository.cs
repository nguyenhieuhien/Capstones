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
    public class CourseRepository : GenericRepository<Course>
    {
        public CourseRepository() { }

        public async Task<List<Course>> GetAll()
        {
            var courses = await _context.Courses.Where(a => a.IsActive == true).ToListAsync();

            return courses;
        }

        public async Task<List<Course>> GetAllByOrgId(Guid orgId)
        {
            var courses = await _context.Courses
                .Where(c => c.WorkSpace.OrganizationId == orgId && c.IsActive == true)
                .ToListAsync();

            return courses;
        }

        public async Task<string?> GetInstructorFullNameByCourseIdAsync(Guid courseId)
        {
            return await _context.Courses
                .Where(c => c.Id == courseId)
                .Select(c => c.Instructor.FullName) // trả về string
                .FirstOrDefaultAsync();
        }


        public async Task<Course?> GetCourseByIdAsync(Guid Id)
        {
            return await _context.Courses
        .Include(c => c.Instructor)   // load luôn Instructor
        .FirstOrDefaultAsync(c => c.Id == Id && c.IsActive == true);
        }

        public async Task<List<Course>> Search(string name, string description)
        {
            var courses = await _context.Courses.Include(t => t.CourseName).Include(t => t.Description).Where(tq =>
            (tq.CourseName.Contains(name) || string.IsNullOrEmpty(name)
            && (tq.Description.Contains(description)) || string.IsNullOrEmpty(description)
            )).ToListAsync();

            return courses;
        }

        public async Task<List<Course>> GetCoursesByInstructorIdAsync(Guid instructorId)
        {
            return await _context.Courses
                .Where(c => c.InstructorId == instructorId && c.IsActive == true)
                .ToListAsync();
        }

        public async Task<List<Course>> GetCoursesByCategoryIdAsync(Guid categoryId)
        {
            return await _context.Courses
                .Where(c => c.CategoryId == categoryId && c.IsActive == true)
                .ToListAsync();
        }

        public async Task<List<Course>> GetAllByWorkspaceId(Guid workspaceId)
        {
            var courses = await _context.Courses
                .Where(c => c.WorkSpaceId == workspaceId && c.IsActive == true)
                .ToListAsync();

            return courses;
        }
        public async Task<List<Course>> GetAllByCategoryId(Guid categoryId)
        {
            var courses = await _context.Courses
                .Where(c => c.CategoryId == categoryId && c.IsActive == true)
                .ToListAsync();
            return courses;
        }
    }
}
