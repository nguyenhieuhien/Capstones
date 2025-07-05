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

        public async Task<bool> IsStudentInClass(Guid classId, Guid studentId)
        {
            return await _context.AccountOfClasses
                .AnyAsync(x => x.ClassId == classId && x.AccountId == studentId);
        }

        public async Task<AccountOfClass?> GetStudentInClass(Guid classId, Guid studentId)
        {
            return await _context.AccountOfClasses
                .FirstOrDefaultAsync(x => x.ClassId == classId && x.AccountId == studentId);
        }

        public async Task<List<AccountOfClass>> GetByClassIdAsync(Guid classId)
        {
            return await _context.AccountOfClasses
                .Where(a => a.ClassId == classId)
                .ToListAsync();
        }
    }
}
