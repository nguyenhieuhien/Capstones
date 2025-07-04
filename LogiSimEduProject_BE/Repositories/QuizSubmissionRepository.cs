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

        public async Task<List<Answer>> GetCorrectAnswersByQuiz(Guid quizId)
        {
            return await _context.Answers
                    .Include(a => a.Question)
                    .Where(a => a.Question.QuizId == quizId && a.IsAnswerCorrect == true)
                    .ToListAsync();
        }

        public async Task AddSubmissionAnswers(List<QuizSubmissionAnswer> answers)
        {
            await _context.QuizSubmissionAnswers.AddRangeAsync(answers);
            await _context.SaveChangesAsync();
        }
    }
}
