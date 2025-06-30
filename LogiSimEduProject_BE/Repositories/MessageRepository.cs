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
    public class MessageRepository : GenericRepository<Message>
    {
        public MessageRepository() { }

        public async Task<List<Message>> GetAll()
        {
            var messages = await _context.Messages.ToListAsync();

            return messages;
        }
    }
}
