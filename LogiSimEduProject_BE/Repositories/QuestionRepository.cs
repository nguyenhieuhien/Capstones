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
    }
}
