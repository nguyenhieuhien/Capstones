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
    public class TopicRepository : GenericRepository<Topic>
    {
        public TopicRepository() { }

        public async Task<List<Topic>> GetAll()
        {
            var topics = await _context.Topics.ToListAsync();

            return topics;
        }
    }
}
