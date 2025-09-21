using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface IEnrollmentWorkSpaceService
    {
        Task<List<EnrollmentWorkSpace>> GetAll();
        Task<EnrollmentWorkSpace> GetById(string id);
        Task<(bool Success, string Message)> Create(EnrollmentWorkSpace accWs);
        Task<(bool Success, string Message)> Update(EnrollmentWorkSpace accWs);
        Task<(bool Success, string Message)> Delete(string id);
    }
}
