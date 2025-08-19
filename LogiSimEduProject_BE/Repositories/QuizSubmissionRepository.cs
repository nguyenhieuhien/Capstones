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
    }
}
