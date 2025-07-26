using Repositories.Models;
using Services.DTO.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface IOrderService
    {
        Task<List<Order>> GetAllAsync();
        Task<Order?> GetByIdAsync(Guid id);
        Task<Order> CreateAsync(OrderDTOCreate dto);
        //Task<Order?> UpdateAsync(Guid id, OrderDTOUpdate dto);
        Task<bool> DeleteAsync(Guid id);
    }

}
