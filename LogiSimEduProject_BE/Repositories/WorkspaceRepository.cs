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
    public class WorkspaceRepository : GenericRepository<Workspace>
    {
        public WorkspaceRepository() { }

        public async Task<List<Workspace>> GetAll()
        {
            var workspaces = await _context.Workspaces.ToListAsync();

            return workspaces;
        }
    }
}
