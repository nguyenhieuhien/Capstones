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
    public class ObjectRepository : GenericRepository<ObjectModel>
    {
        public ObjectRepository() { }
        public async Task<List<ObjectModel>> GetAll()
        {
            var objects = await _context.ObjectModels.Where(a => a.IsActive == true).ToListAsync();

            return objects;
        }


    }
}
