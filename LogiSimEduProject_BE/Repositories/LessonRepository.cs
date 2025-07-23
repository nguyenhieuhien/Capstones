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
    public class LessonRepository : GenericRepository<Lesson>
    {
        public LessonRepository() { }
        public async Task<List<Lesson>> GetAll()
        {
            var lessons = await _context.Lessons.ToListAsync();

            return lessons;
        }
    }
}
