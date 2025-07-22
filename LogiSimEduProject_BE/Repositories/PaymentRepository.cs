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
   
        public class PaymentRepository : GenericRepository<Payment>
        {
            public PaymentRepository()
            {
            }

        public new async Task<List<Payment>> GetAll()
        {
            var payments = await _context.Payments.ToListAsync();
            return payments ?? new List<Payment>(); // đảm bảo không null
        }
    }
}
