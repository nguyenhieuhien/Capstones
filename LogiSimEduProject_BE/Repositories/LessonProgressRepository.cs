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
            var lessonProgresses = await _context.LessonProgresses.Where(a => a.IsActive == true).ToListAsync();

            return lessonProgresses;
        }
        public async Task<int> CountCompletedLessonsAsync(Guid accountId, List<Guid> lessonIds)
        {
            return await _context.LessonProgresses
                .CountAsync(lp => lessonIds.Contains(lp.LessonId.Value)
                                && lp.AccountId == accountId
                                && lp.Status == 2
                                && lp.IsActive == true);
        }

        public async Task<LessonProgress?> GetByAccountAndLesson(Guid accountId, Guid lessonId)
        {
            return await _context.LessonProgresses
                .FirstOrDefaultAsync(lp => lp.AccountId == accountId && lp.LessonId == lessonId && lp.IsActive == true);
        }

        public async Task<bool> ExistsAsync(Guid accountId, Guid lessonId)
        {
            return await _context.LessonProgresses
                .AnyAsync(lp => lp.AccountId == accountId && lp.LessonId == lessonId && lp.IsActive == true);
        }

        public async Task<int> Created(LessonProgress lessonProgress)
        {
            await _context.LessonProgresses.AddAsync(lessonProgress);
            return await _context.SaveChangesAsync();
        }
    }
}
