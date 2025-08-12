using Repositories.Models;
using Services.DTO.Certificate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface ILessonProgressService
    {
        Task<List<LessonProgress>> GetAll();
        Task<LessonProgress?> GetById(string id);
        Task<(bool Success, string Message, Guid? Id)> Create(LessonProgress lessonProgress);
        Task<(bool Success, string Message, CertificateDTO? Certificate)> UpdateLessonProgressAsync(Guid accountId, Guid lessonId, int status);
        Task<(bool Success, string Message)> Update(LessonProgress lessonProgress);
        Task<(bool Success, string Message)> Delete(string id);
    }
}
