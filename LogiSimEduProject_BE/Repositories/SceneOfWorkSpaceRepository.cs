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
    public class SceneOfWorkSpaceRepository : GenericRepository<SceneOfWorkSpace>
    {
        public SceneOfWorkSpaceRepository() { }

        public async Task<List<SceneOfWorkSpace>> GetAll()
        {
            var scWs = await _context.SceneOfWorkSpaces.ToListAsync();

            return scWs;
        }

        public async Task<List<SceneOfWorkSpace>> GetBySceneIdAsync(Guid sceneId)
        {
            return await _context.SceneOfWorkSpaces
                .Where(a => a.SceneId == sceneId)
                .ToListAsync();
        }
    }
}
