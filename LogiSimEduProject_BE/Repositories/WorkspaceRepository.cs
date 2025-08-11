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
    public class WorkspaceRepository : GenericRepository<WorkSpace>
    {
        public WorkspaceRepository() { }

        public async Task<List<WorkSpace>> GetAll()
        {
            var workspaces = await _context.WorkSpaces.Where(a => a.IsActive == true).ToListAsync();

            return workspaces;
        }

        public async Task<List<WorkSpace>> GetAllByOrgId(Guid orgId)
        {
            var workspaces = await _context.WorkSpaces
                .Where(w => w.OrganizationId == orgId && w.IsActive == true)
                .ToListAsync();

            return workspaces;
        }
    }
}
