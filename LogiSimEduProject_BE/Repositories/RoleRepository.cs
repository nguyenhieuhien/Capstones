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
    public class RoleRepository : GenericRepository<Role>
    {
        public RoleRepository() { }

        public async Task<List<Role>> GetAll()
        {
            var roles = await _context.Roles.ToListAsync();

            return roles;
        }
    }
}
