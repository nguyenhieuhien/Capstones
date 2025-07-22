using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface IQuestionService
    {
        Task<List<Question>> GetAll();
        Task<Question?> GetById(string id);
        Task<(bool Success, string Message, Guid? Id)> Create(Question question);
        Task<(bool Success, string Message)> Update(Question question);
        Task<(bool Success, string Message)> Delete(string id);
    }

}
