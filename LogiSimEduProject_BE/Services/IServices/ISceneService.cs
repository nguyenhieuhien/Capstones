using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface ISceneService
    {
        Task<List<Scene>> GetAll();
        Task<Scene?> GetById(string id);
        Task<int> Create(Scene scene);
        Task<int> Update(Scene scene);
        Task<bool> Delete(string id);
    }
}
