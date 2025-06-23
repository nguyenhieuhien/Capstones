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
    public class ScenarioRepository : GenericRepository<Scenario>
    {
        public ScenarioRepository() { }
        public async Task<List<Scenario>> GetAll()
        {
            var scenarios = await _context.Scenarios.ToListAsync();

            return scenarios;
        }
    }
}
