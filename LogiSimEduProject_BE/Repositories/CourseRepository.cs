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
            var courses = await _context.Courses.ToListAsync();

            return courses;
        }

        public async Task<List<Course>> Search(string name, string description)
        {
            var courses = await _context.Courses.Include(t => t.Name).Include(t => t.Description).Where(tq =>
            (tq.Name.Contains(name) || string.IsNullOrEmpty(name)
            && (tq.Description.Contains(description)) || string.IsNullOrEmpty(description)
            )).ToListAsync();

            return courses;
        }
    }
}
