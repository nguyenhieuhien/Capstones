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
    public class QuizRepository : GenericRepository<Quiz>
    {
        public QuizRepository() { }

        public async Task<List<Quiz>> GetAll()
        {
            var quizzes = await _context.Quizzes.Where(a => a.IsActive == true).ToListAsync();

            return quizzes;
        }

        public async Task<List<Question>> GetQuestionsWithAnswersByQuizId(Guid quizId)
        {
            return await _context.Questions
                .Include(q => q.Answers)
                .Where(q => q.QuizId == quizId && q.IsActive == true)
                .ToListAsync();
        }

        public async Task<List<QuestionSubmission>> GetQuestionSubmissions(Guid accountId, Guid quizId)
        {
            return await _context.QuestionSubmissions
                .Where(qs => qs.QuizSubmission.AccountId == accountId
                          && qs.QuizSubmission.QuizId == quizId
                          && qs.IsActive == true)
                .ToListAsync();
        }

        public async Task<Quiz?> GetFullQuizAsync(Guid quizId)
        {
            return await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qs => qs.Answers)
                .FirstOrDefaultAsync(q => q.Id == quizId && q.IsActive == true);
        }

        public async Task<List<Quiz>> GetQuizByLessonId(Guid lessonId)
        {
            return await _context.Quizzes
                .Where(q => q.LessonId == lessonId && q.IsActive == true)
                .ToListAsync();
        }
    }
}
