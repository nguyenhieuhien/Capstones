using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface IPaymentService
    {
        Task<Payment> GetByOrderCodeAsync(long? orderCode);
        Task CreatePaymentAsync(Payment payment);
        Task UpdatePaymentAsync(Payment payment);
        Task<IEnumerable<Payment>> GetAllAsync();
        Task<Payment?> GetByIdAsync(Guid id);

    }
}
