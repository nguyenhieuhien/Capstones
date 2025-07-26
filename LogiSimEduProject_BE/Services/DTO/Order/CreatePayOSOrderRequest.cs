using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO.Order
{
    public class CreatePayOSOrderRequest
    {
        public string OrderCode { get; set; }              // Mã đơn hàng nội bộ (string guid)
        public int Amount { get; set; }                    // Tiền thanh toán (VNĐ, kiểu int)
        public string Description { get; set; }
        public string ReturnUrl { get; set; }              // URL redirect sau khi thanh toán thành công
        public string CancelUrl { get; set; }              // URL redirect khi hủy
    }
}
