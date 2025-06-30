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
    public class ConversationParticipantRepository : GenericRepository<ConversationParticipant>
    {
        public ConversationParticipantRepository() { }

        public async Task<List<ConversationParticipant>> GetAll()
        {
            var converPar = await _context.ConversationParticipants.ToListAsync();

            return converPar;
        }
    }
}
