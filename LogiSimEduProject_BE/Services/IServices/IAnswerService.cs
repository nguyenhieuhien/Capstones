using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface IAnswerService
    {
        Task<List<Answer>> GetAll();
        Task<Answer> GetById(string id);
        Task<List<Answer>> GetAllAnswersByQuestionId(Guid questionId);
        Task<(bool Success, string Message)> Create(Answer answer);
        Task<(bool Success, string Message)> Update(Answer answer);
        Task<(bool Success, string Message)> Delete(string id);
    }
}
