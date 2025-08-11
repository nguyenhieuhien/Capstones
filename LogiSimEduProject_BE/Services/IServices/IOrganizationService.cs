using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface IOrganizationService
    {
        Task<List<Organization>> GetAll();
        Task<Organization?> GetById(string id);
        Task<(bool Success, string Message, Guid? Id)> Create(Organization organization);
        Task<(bool Success, string Message)> Update(Organization organization);
        Task<bool> UpdateActiveAsync(Guid organizationId, bool isActive);
        Task<(bool Success, string Message)> Delete(string id);
    }

}
