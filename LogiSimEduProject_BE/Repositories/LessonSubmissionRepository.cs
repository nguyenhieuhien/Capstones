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

        public async Task<List<LessonSubmission>> GetByLessonIdAsync(Guid lessonId)
        {
            return await _context.LessonSubmissions
                .Include(s => s.Account)
                    .ThenInclude(a => a.AccountOfCourses)
                        .ThenInclude(ac => ac.Class)
                .Where(s => s.LessonId == lessonId && s.IsActive)
                .ToListAsync();
        }

        public async Task<LessonSubmission?> GetLessonSubmissionAsync(Guid lessonId, Guid accountId)
        {
            return await _context.LessonSubmissions
                .Include(ls => ls.Account)   // include Account
                .Where(ls => ls.LessonId == lessonId
                          && ls.AccountId == accountId
                          && ls.IsActive)
                .FirstOrDefaultAsync();
        }
    }
}
