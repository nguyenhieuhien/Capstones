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
            var quizzes = await _context.Quizzes.ToListAsync();

            return quizzes;
        }
    }
}
