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
    public class AccountOfClassRepository : GenericRepository<AccountOfClass>
    {
        public AccountOfClassRepository() { }

        public async Task<List<AccountOfClass>> GetAll()
        {
            var acCl = await _context.AccountOfClasses.ToListAsync();

            return acCl;
        }
    }
}
