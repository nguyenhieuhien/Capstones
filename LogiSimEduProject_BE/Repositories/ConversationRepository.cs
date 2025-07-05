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

        public async Task<List<Conversation>> GetConversationsByUserId(Guid userId)
        {
            return await _context.Conversations
                .Where(c => c.ConversationParticipants.Any(p => p.AccountId == userId))
                .ToListAsync();
        }

        public async Task<Conversation> FindOneToOneConversation(Guid userId1, Guid userId2)
        {
            return await _context.Conversations
                .Include(c => c.ConversationParticipants)
                .FirstOrDefaultAsync(c =>
                    !c.IsGroup &&
                    c.ConversationParticipants.Any(p => p.AccountId == userId1) &&
                    c.ConversationParticipants.Any(p => p.AccountId == userId2));
        }
    }
}
