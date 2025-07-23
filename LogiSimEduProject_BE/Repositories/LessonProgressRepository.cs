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
    public class LessonProgressRepository : GenericRepository<LessonProgress>
    {
        public LessonProgressRepository() { }
        public async Task<List<LessonProgress>> GetAll()
        {
            var lessonProgresses = await _context.LessonProgresses.ToListAsync();

            return lessonProgresses;
        }
    }
}
