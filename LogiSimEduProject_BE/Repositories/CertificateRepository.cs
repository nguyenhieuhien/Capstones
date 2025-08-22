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
    public class CertificateRepository : GenericRepository<Certificate>
    {
        public CertificateRepository() { }

        public async Task<List<Certificate>> GetAll()
        {
            return await _context.Certificates.Where(c => c.IsActive == true).ToListAsync();
        }

        public async Task<List<Certificate>> GetByAccountIdAsync(Guid accountId)
        {
            return await _context.Certificates
                .Where(c => c.AccountId == accountId && c.IsActive == true)
                .ToListAsync();
        }

        public async Task<Certificate?> GetByCourseIdAsync(Guid courseId)
        {
            return await _context.Certificates
                .Where(c => c.CourseId == courseId && c.IsActive == true)
                .FirstOrDefaultAsync();
        }


        public async Task<Certificate?> GetCertificateByCourseIdAndAccIdAsync(Guid courseId, Guid accountId)
        {
            return await _context.Certificates
                .Include(c => c.Course)
                .Include(c => c.Account)
                .FirstOrDefaultAsync(c => c.CourseId == courseId
                                       && c.AccountId == accountId
                                       && c.IsActive == true);
        }

        public async Task<List<Certificate>> GetByAccountAndCourse(Guid accountId, Guid courseId)
        {
            return await _context.Certificates
                .Where(c => c.AccountId == accountId && c.CourseId == courseId && c.IsActive == true)
                .ToListAsync();
        }
    }
}
