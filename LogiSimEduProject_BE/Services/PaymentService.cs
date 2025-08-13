using Repositories;
using Repositories.Models;
using Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class PaymentService : IPaymentService
    {
        private readonly PaymentRepository _paymentRepository;

        public PaymentService()
        {
            _paymentRepository = new PaymentRepository();
        }
        public async Task<Payment> GetByOrderCodeAsync(long? orderCode)
        {
            return await _paymentRepository.GetByOrderCodeAsync(orderCode);
        }
        public async Task CreatePaymentAsync(Payment payment)
        {
            if (payment == null)
            {
                throw new ArgumentNullException(nameof(payment));
            }

            var existingPayment = await _paymentRepository.GetByOrderCodeAsync(payment.OrderCode);
            if (existingPayment != null)
            {
                throw new InvalidOperationException($"Payment with OrderCode {payment.OrderCode} already exists.");
            }

            await _paymentRepository.CreatePaymentAsync(payment);
        }
        public async Task UpdatePaymentAsync(Payment payment)
        {
            await _paymentRepository.UpdatePaymentAsync(payment);
        }


        public async Task<IEnumerable<Payment>> GetAllAsync() =>
            await _paymentRepository.GetAll();

        public async Task<Payment?> GetByIdAsync(Guid id) =>
            await _paymentRepository.GetById(id);
    }
}
