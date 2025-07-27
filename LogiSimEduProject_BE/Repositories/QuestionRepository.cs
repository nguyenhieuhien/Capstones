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
    public class QuestionRepository : GenericRepository<Question>
    {
        public QuestionRepository() { }

        public async Task<List<Question>> GetAll()
        {
            var questions = await _context.Questions.ToListAsync();

            return questions;
        }

        public async Task<Question> GetByIdAsync(string id)
        {
            return await _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id.ToString() == id);
        }

        public async Task<List<Question>> GetQuestionsWithAnswersByQuizId(Guid quizId)
        {
            return await _context.Questions
                .Where(q => q.QuizId == quizId && q.IsActive == true)
                .Include(q => q.Answers.Where(a => a.IsActive == true))
                .ToListAsync();
        }
    }
}
