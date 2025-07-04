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
    public class PackageTypeRepository : GenericRepository<PackageType>
    {
        public PackageTypeRepository() { }

        public async Task<List<PackageType>> GetAll()
        {
            var packageTypes = await _context.PackageTypes.ToListAsync();

            return packageTypes;
        }
    }
}
