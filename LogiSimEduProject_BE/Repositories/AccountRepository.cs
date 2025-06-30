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

        public async Task<Account?> GetAccountByEmail(string email)
        {
            return await _context.Accounts.FirstOrDefaultAsync(u => u.Email == email);
        }


        public async Task<Account> GetByEmailAsync(string email)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
        }

        public async Task<Role> GetRoleByNameAsync(string roleName)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
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

        public async Task<List<Account>> Search(string username, string fullname, string email, string phone)
        {
            var accounts = await _context.Accounts.Include(t => t.UserName).Include(t => t.FullName).Include(t => t.Email).Include(t => t.Phone).Where(tq =>
            (tq.UserName.Contains(username) || string.IsNullOrEmpty(username)
            && (tq.FullName.Contains(fullname)) || string.IsNullOrEmpty(fullname)
            && (tq.Email.Contains(email)) || string.IsNullOrEmpty(email)
            && (tq.FullName.Contains(phone)) || string.IsNullOrEmpty(phone)
            )).ToListAsync();

            return accounts;
        }
    }
}
