using Microsoft.EntityFrameworkCore;
using Repositories.Base;
using Repositories.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public class OrganizationRepository : GenericRepository<Organization>
{
    public OrganizationRepository() { }

    public new async Task<List<Organization>> GetAll()
    {
        var organizations = await _context.Organizations.ToListAsync();
        return organizations ?? new List<Organization>(); // đảm bảo không null
    }
}
