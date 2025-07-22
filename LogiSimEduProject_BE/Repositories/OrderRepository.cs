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
        public class OrderRepository : GenericRepository<Order>
        {
            public OrderRepository() { }

        public async Task<List<Order>> GetAll()
        {
            var orders = await _context.Orders.ToListAsync();

            return orders;
        }
    }
    }
