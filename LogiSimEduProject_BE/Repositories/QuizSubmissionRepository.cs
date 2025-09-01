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
    public class QuizSubmissionRepository : GenericRepository<QuizSubmission>
    {
        public QuizSubmissionRepository() { }

        public async Task<QuizSubmission?> GetLatestByAccountAndQuiz(Guid accountId, Guid quizId)
        {
            return await _context.QuizSubmissions
                    .Where(s => s.AccountId == accountId
                    && s.QuizId == quizId
                    && s.IsActive == true)
                    .OrderByDescending(s => s.CreatedAt)
                    .FirstOrDefaultAsync();
        }

        // Lấy tất cả QuizSubmission theo QuizId
        public async Task<List<QuizSubmission>> GetByQuizIdAsync(Guid quizId)
        {
            return await _context.QuizSubmissions
                .Where(qs => qs.QuizId == quizId && qs.IsActive == true)
                .ToListAsync();
        }

        public async Task<List<QuizSubmission>> GetLessonQuizSubmissions(Guid quizId)
        {
            return await _context.QuizSubmissions
                .Include(qs => qs.Account)
                    .ThenInclude(a => a.AccountOfCourses)
                        .ThenInclude(aoc => aoc.Class)
                .Include(qs => qs.Quiz)
                    .ThenInclude(q => q.Lesson)
                        .ThenInclude(l => l.Topic)
                .Where(qs => qs.Quiz.Id == quizId && qs.IsActive == true)
                .ToListAsync();
        }

        public async Task<Dictionary<Guid, double?>> GetLatestScoresByQuizForTopicAsync(Guid topicId, Guid accountId)
        {
            // Lấy mỗi quiz 1 record: submission mới nhất của account trong topic
            var latestPerQuiz = await _context.QuizSubmissions
                .AsNoTracking()
                .Where(qs => (qs.IsActive ?? false)
                             && qs.AccountId == accountId
                             && qs.QuizId != null
                             && qs.Quiz!.Lesson.TopicId == topicId)
                .GroupBy(qs => qs.QuizId!.Value)
                .Select(g => g.OrderByDescending(x => (x.SubmitTime ?? x.CreatedAt))
                              .Select(x => new { x.QuizId, x.TotalScore })
                              .FirstOrDefault()!)
                .ToDictionaryAsync(x => x.QuizId!.Value, x => x.TotalScore);

            return latestPerQuiz;
        }
    }
}
