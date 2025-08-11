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
            var lessons = await _context.Lessons.Where(a => a.IsActive == true).ToListAsync();

            return lessons;
        }

        public async Task<List<Lesson>> GetLessonsByTopicIdAsync(Guid topicId)
        {
            return await _context.Lessons
                .Where(l => l.TopicId == topicId && l.IsActive == true)
                .ToListAsync();
        }

        public async Task<List<Lesson>> GetLessonsByCourseId(Guid courseId)
        {
            return await _context.Lessons
                .Include(l => l.Topic)
                .Where(l => l.Topic.CourseId == courseId && l.IsActive == true)
                .ToListAsync();
        }

        public async Task<List<Quiz>> GetQuizzesByLessonId(Guid lessonId)
        {
            return await _context.Quizzes
                .Where(q => q.LessonId == lessonId && q.IsActive == true)
                .ToListAsync();
        }
    }
}
