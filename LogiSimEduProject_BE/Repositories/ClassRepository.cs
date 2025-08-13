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
    }
}
