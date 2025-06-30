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
    public class ClassRepository : GenericRepository<Class>
    {
        public ClassRepository() { }

        public async Task<List<Class>> GetAll()
        {
            var classes = await _context.Classes.ToListAsync();

            return classes;
        }
    }
}
