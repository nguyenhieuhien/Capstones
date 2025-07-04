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
    public class PackageRepository : GenericRepository<Package>
    {
        public PackageRepository() { }

        public async Task<List<Package>> GetAll()
        {
            var packages = await _context.Packages.ToListAsync();

            return packages;
        }
    }
}
