using Repositories.Models;
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
        Task<(bool Success, string Message, Guid? Id)> Create(Quiz quiz);
        Task<(bool Success, string Message)> CreateFullQuiz(Quiz dto);
        Task<(bool Success, string Message)> Update(Quiz quiz);
        Task<(bool Success, string Message)> Delete(string id);
    }
}
