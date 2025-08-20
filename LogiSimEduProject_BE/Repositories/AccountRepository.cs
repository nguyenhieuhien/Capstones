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

        public async Task<Account?> GetAccountByUserName(string username)
        {
            return await _context.Accounts.FirstOrDefaultAsync(u => u.UserName == username);
        }


        public async Task<Account> GetByEmailAsync(string email)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.Email.ToLower() == email.ToLower());
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

        public async Task<Account> GetById(string id)
        {
            return await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id.ToString() == id);
        }

        public async Task<List<Account>> GetAllByOrgId(Guid orgId)
        {
            var accounts = await _context.Accounts
                .Where(a => a.OrganizationId == orgId && a.IsActive == true)
                .ToListAsync();

            return accounts;
        }

        public async Task<List<Account>> GetAccountsByRoleAsync(Guid organizationId, string roleName)
        {
            return await _context.Accounts
                .Include(a => a.Role)
                .Include(a => a.GenderNavigation)
                .Where(a => a.OrganizationId == organizationId && a.Role.Name == roleName && a.IsActive == true)
                .ToListAsync();
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
