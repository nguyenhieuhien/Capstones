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

        public async Task<Payment> GetByOrderCodeAsync(long? orderCode)
        {
            if (orderCode == null)
            {
                return null; // Hoặc throw new ArgumentNullException(nameof(orderCode));
            }
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderCode == orderCode);
        }

        public async Task CreatePaymentAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
        }
        public async Task UpdatePaymentAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                Console.WriteLine($"Payment updated successfully for orderCode: {payment.OrderCode}");
            }
            else
            {
                Console.WriteLine($"Failed to update payment for orderCode: {payment.OrderCode}");
            }
        }
        public async Task<IEnumerable<Payment>> GetAll()
        {
            return await _context.Payments
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public Task<Payment?> GetById(Guid id)
        {
            return _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

    }
}
