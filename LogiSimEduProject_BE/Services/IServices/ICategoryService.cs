using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAll();
        Task<Category> GetById(string id);
        Task<(bool Success, string Message)> Create(Category category);
        Task<(bool Success, string Message)> Update(Category category);
        Task<(bool Success, string Message)> Delete(string id);
    }
}
