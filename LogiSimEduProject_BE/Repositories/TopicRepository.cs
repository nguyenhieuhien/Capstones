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
    public class TopicRepository : GenericRepository<Topic>
    {
        public TopicRepository() { }

        public async Task<List<Topic>> GetAll()
        {
            var topics = await _context.Topics.Where(a => a.IsActive == true).ToListAsync();

            return topics;
        }

        public async Task<List<Topic>> GetTopicsByCourseIdAsync(Guid courseId)
        {
            return await _context.Topics
                .Where(t => t.CourseId == courseId && t.IsActive == true)
                .OrderBy(t => t.OrderIndex)
                .ToListAsync();
        }

        public async Task<List<Topic>> GetProcessTopicsByCourseIdAsync(
    Guid courseId,
    Guid? accountId = null,     // lọc theo 1 student (tùy chọn)
    int? progressStatus = null  // lọc theo trạng thái progress (tùy chọn, ví dụ Completed=3)
)
        {
            return await _context.Topics
                .AsNoTracking()
                .Where(t => t.CourseId == courseId && t.IsActive == true)

                // Lọc lesson active
                .Include(t => t.Lessons
                    .Where(l => (l.IsActive ?? false)))

                // Lọc progress theo điều kiện (account/status/isActive)
                .ThenInclude(l => l.LessonProgresses
                    .Where(lp =>
                        (lp.IsActive ?? false) &&
                        (accountId == null || lp.AccountId == accountId) &&
                        (progressStatus == null || lp.Status == progressStatus)
                    ))

                // (tùy chọn) tránh Cartesian explosion khi nhiều collection
                .AsSplitQuery()

                // sắp xếp ổn định
                .OrderBy(t => t.OrderIndex)
                .ToListAsync();
        }


        public async Task<Topic?> GetByCourseAndOrderIndexAsync(Guid courseId, int orderIndex)
        {
            return await _context.Topics
                .FirstOrDefaultAsync(t => t.CourseId == courseId && t.OrderIndex == orderIndex && t.IsActive == true);
        }
    }
}
