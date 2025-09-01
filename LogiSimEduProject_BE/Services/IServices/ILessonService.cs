using Repositories.Models;
using Services.DTO.Lesson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface ILessonService
    {
        Task<List<Lesson>> GetAll();
        Task<Lesson?> GetById(string id);
        Task<List<Lesson>> GetLessonsByTopicId(Guid topicId);
        Task<List<QuizDTOByLesson>> GetQuizzesByLessonId(Guid lessonId);
        Task<List<LessonWithQuizzesDTO>> GetLessonsWithLatestScoresAsync(Guid topicId, Guid? accountId = null);
        Task<(bool Success, string Message, Guid? Id)> Create(Lesson lesson);
        Task<(bool Success, string Message)> Update(Lesson lesson);
        Task<(bool Success, string Message)> Delete(string id);
    }
}
