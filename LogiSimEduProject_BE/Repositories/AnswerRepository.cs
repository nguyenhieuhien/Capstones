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
    public class AnswerRepository : GenericRepository<Answer>
    {
        public AnswerRepository() { }

        public async Task<List<Answer>> GetAll()
        {
            var answers = await _context.Answers.Where(a => a.IsActive == true).ToListAsync();

            return answers;
        }
        
        public async Task<List<Answer>> GetAllAnswersByQuestionId(Guid questionId)
        {
            return await _context.Answers
                .Where(a => a.QuestionId == questionId && a.IsActive == true)
                .ToListAsync();
        }
    }
}
