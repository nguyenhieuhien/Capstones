using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface IQuizSubmissionService
    {
        Task<int> SubmitQuiz(Guid quizId, Guid accountId, List<(Guid questionId, Guid answerId)> answers);
    }
}
