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
                .ToListAsync();
        }

        public async Task<Topic?> GetByCourseAndOrderIndexAsync(Guid courseId, int orderIndex)
        {
            return await _context.Topics
                .FirstOrDefaultAsync(t => t.CourseId == courseId && t.OrderIndex == orderIndex && t.IsActive == true);
        }
    }
}
