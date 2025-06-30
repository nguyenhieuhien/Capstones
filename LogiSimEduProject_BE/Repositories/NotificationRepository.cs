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
    public class NotificationRepository : GenericRepository<Notification>
    {
        public NotificationRepository() { }

        public async Task<List<Notification>> GetAll()
        {
            var notifications = await _context.Notifications.ToListAsync();

            return notifications;
        }
    }
}
