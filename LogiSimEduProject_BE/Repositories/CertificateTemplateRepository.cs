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
    public class CertificateTemplateRepository : GenericRepository<CertificateTemplete>
    {
        public CertificateTemplateRepository() { }

        public async Task<CertificateTemplete> GetByCourseIdAsync(Guid courseId)
        {
            return await _context.CertificateTempletes
                .FirstOrDefaultAsync(x => x.CourseId == courseId && x.IsActive == true);
        }

        public async Task<List<CertificateTemplete>> GetAllByOrgIdAsync(Guid orgId)
        {
            return await _context.CertificateTempletes
                .Where(x => x.OrganizationId == orgId && x.IsActive == true)
                .ToListAsync();
        }
    }
}
