using Microsoft.EntityFrameworkCore;
using Repositories.Base;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class EnrollmentWorkSpaceRepository : GenericRepository<EnrollmentWorkSpace>
    {
        public EnrollmentWorkSpaceRepository() { }

        public async Task<List<EnrollmentWorkSpace>> GetAll()
        {
            var accWs = await _context.EnrollmentWorkSpaces.ToListAsync();

            return accWs;
        }

        public async Task<List<EnrollmentWorkSpace>> GetByWorkspaceIdAsync(Guid workSpaceId)
        {
            return await _context.EnrollmentWorkSpaces
                .Where(a => a.WorkSpaceId == workSpaceId)
                .ToListAsync();
        }
    }
}
