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
    public class AccountRepository : GenericRepository<Account>
    {
        public AccountRepository() { }

        public async Task<Account> GetUserAccount(string email, string password)
        {
            return await _context.Accounts.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
        }

        public async Task<Account> GetByEmailAsync(string email)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
        }

        public async Task InsertAsync(Account account)
        {
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Account>> GetAll()
        {
            var accounts = await _context.Accounts.ToListAsync();

            return accounts;
        }


    }
}
