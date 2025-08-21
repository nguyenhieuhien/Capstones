using Repositories.Models;
using Services.DTO.Question;
using Services.DTO.Quiz;
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
        Task<Quiz?> GetFullQuizAsync(Guid quizId);
        Task<List<QuestionWithAnswersDTO>> GetQuestionsWithAnswersByQuizId(Guid quizId);
        Task<List<Quiz>> GetByLessonId(Guid lessonId);
        Task<(bool Success, string Message, Guid? Id)> Create(Quiz quiz);
        Task<(bool Success, string Message)> CreateFullQuiz(Quiz dto);
        Task<(bool Success, string Message)> UpdateFullQuizAsync(Guid quizId, UpdateFullQuizDTO request);
        //Task<bool> UpdateFullQuizAsync(Quiz quiz);
        Task<List<QuizReviewDTO>> GetQuizReview(Guid accountId, Guid quizId);
        Task<(bool Success, string Message)> Update(Quiz quiz);
        Task<(bool Success, string Message)> Delete(string id);
    }
}
