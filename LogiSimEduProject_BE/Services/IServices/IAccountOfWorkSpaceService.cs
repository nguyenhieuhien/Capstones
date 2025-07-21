using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface IAccountOfWorkSpaceService
    {
        Task<List<AccountOfWorkSpace>> GetAll();
        Task<AccountOfWorkSpace> GetById(string id);
        Task<(bool Success, string Message)> Create(AccountOfWorkSpace accWs);
        Task<(bool Success, string Message)> Update(AccountOfWorkSpace accWs);
        Task<(bool Success, string Message)> Delete(string id);
    }
}
