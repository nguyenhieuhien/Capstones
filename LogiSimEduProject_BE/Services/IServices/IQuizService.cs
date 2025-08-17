using Repositories.Models;
using Services.DTO.Question;
using Services.DTO.QuizSubmission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface IQuizService
    {
        Task<List<Quiz>> GetAll();
        Task<Quiz?> GetById(string id);
        Task<List<QuestionWithAnswersDTO>> GetQuestionsWithAnswersByQuizId(Guid quizId);
        Task<List<Quiz>> GetByLessonId(Guid lessonId);
        Task<(bool Success, string Message, Guid? Id)> Create(Quiz quiz);
        Task<(bool Success, string Message)> CreateFullQuiz(Quiz dto);
        Task<List<QuizReviewDTO>> GetQuizReview(Guid accountId, Guid quizId);
        Task<(bool Success, string Message)> Update(Quiz quiz);
        Task<(bool Success, string Message)> Delete(string id);
    }
}
