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
    public class SceneRepository : GenericRepository<Scene>
    {
        public SceneRepository() { }

        public async Task<List<Scene>> GetAll()
        {
            var scenes = await _context.Scenes.Where(a => a.IsActive == true).ToListAsync();

            return scenes;
        }

        public async Task<List<Scene>> GetAllByOrgId(Guid orgId)
        {
            var scenes = await _context.Scenes
                .Where(s => s.SceneOfWorkSpaces.Any(sw => sw.WorkSpace.OrganizationId == orgId) && s.IsActive == true)
                .ToListAsync();

            return scenes;
        }
    }
}
