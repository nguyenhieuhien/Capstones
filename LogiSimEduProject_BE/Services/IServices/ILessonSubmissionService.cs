using Repositories.Models;
using Services.DTO.LessonSubmission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface ILessonSubmissionService
    {
        Task<List<LessonSubmission>> GetAll();
        Task<LessonSubmission?> GetById(string id);
        Task<Dictionary<string, List<StudentSubmissionDTO>>> GetGroupedByClassAsync(Guid lessonId);
        Task<(bool Success, string Message, Guid? Id)> SubmitLesson(LessonSubmission lessonSubmission);
        Task<(bool Success, string Message)> Update(LessonSubmission lessonSubmission);
        Task<(bool Success, string Message)> Delete(string id);
    }
}
