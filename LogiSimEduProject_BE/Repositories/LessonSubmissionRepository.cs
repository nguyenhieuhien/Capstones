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
    public class LessonSubmissionRepository : GenericRepository<LessonSubmission>
    {
        public LessonSubmissionRepository() { }

        public async Task<List<LessonSubmission>> GetAll()
        {
            var lessonSubmissions = await _context.LessonSubmissions.Where(a => a.IsActive == true).ToListAsync();

            return lessonSubmissions;
        }
    }
}
