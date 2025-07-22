using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface IWorkspaceService
    {
        Task<List<WorkSpace>> GetAll();
        Task<WorkSpace> GetById(string id);
        Task<int> Create(WorkSpace workspace);
        Task<int> Update(WorkSpace workspace);
        Task<bool> Delete(string id);
    }
}
