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
    public class AccountOfWorkSpaceRepository : GenericRepository<AccountOfWorkSpace>
    {
        public AccountOfWorkSpaceRepository() { }

        public async Task<List<AccountOfWorkSpace>> GetAll()
        {
            var accWs = await _context.AccountOfWorkSpaces.ToListAsync();

            return accWs;
        }
    }
}
