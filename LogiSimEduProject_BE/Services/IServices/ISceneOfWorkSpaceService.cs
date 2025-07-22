using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface ISceneOfWorkSpaceService
    {
        Task<List<SceneOfWorkSpace>> GetAll();
        Task<SceneOfWorkSpace> GetById(string id);
        Task<int> Create(SceneOfWorkSpace scWs);
        Task<int> Update(string id, SceneOfWorkSpace scWs);
        Task<bool> Delete(string id);
    }
}
