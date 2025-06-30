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
    public class ConversationRepository : GenericRepository<Conversation>
    {
        public ConversationRepository() { }
        public async Task<List<Conversation>> GetAll()
        {
            var conversations = await _context.Conversations.ToListAsync();

            return conversations;
        }
    }
}
