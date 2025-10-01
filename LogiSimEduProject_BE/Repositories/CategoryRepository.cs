using Microsoft.EntityFrameworkCore;
using Repositories.Base;
using Repositories.DBContext;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class CategoryRepository : GenericRepository<Category>
    {
        public CategoryRepository() { }

        public new async Task<List<Category>> GetAll()
        {
            var categories = await _context.Categories.Where(a => a.IsActive == true).ToListAsync();
            return categories ?? new List<Category>(); // đảm bảo không null
        }

        public async Task<List<Category>> GetByWorkspaceIdAsync(Guid workspaceId)
        {
            return await _context.Categories
                .Where(c => c.WorkSpaceId == workspaceId && c.IsActive == true)
                .Include(c => c.Courses) // nếu muốn lấy luôn Courses
                .ToListAsync();
        }
    }
}
