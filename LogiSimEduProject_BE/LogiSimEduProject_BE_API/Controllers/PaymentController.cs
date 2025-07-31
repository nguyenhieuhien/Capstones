using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;
using Repositories.Models;
using Services.IServices;

using Swashbuckle.AspNetCore.Annotations;

namespace LogiSimEduProject_BE_API.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {
        private readonly PayOS _payOS;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;
        private readonly IConfiguration _configuration;

        public PaymentController(IConfiguration configuration, IOrderService orderService, IPaymentService paymentService, ILogger<PaymentController> logger)
        {
            _configuration = configuration;
            string clientId = configuration["PayOS:ClientId"];
            string apiKey = configuration["PayOS:ApiKey"];
            string checksumKey = configuration["PayOS:ChecksumKey"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(checksumKey))
            {
                throw new ArgumentException("PayOS configuration is missing or invalid.");
            }

            _payOS = new PayOS(clientId, apiKey, checksumKey);
            _orderService = orderService;
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost("create-payment/{orderId}")]
        [SwaggerOperation(Summary = "Tạo liên kết thanh toán cho đơn hàng")]
        public async Task<IActionResult> CreatePayment(Guid orderId)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning("Không tìm thấy đơn hàng với ID: {OrderId}", orderId);
                    return NotFound(new { message = "Không tìm thấy đơn hàng." });
                }

                if (order.TotalPrice <= 0)
                {
                    _logger.LogWarning("Giá trị thanh toán không hợp lệ. ID: {OrderId}, TotalPrice: {TotalPrice}", orderId, order.TotalPrice);
                    return BadRequest(new { message = "Tổng giá trị đơn hàng phải lớn hơn 0." });
                }

                var orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                var items = new List<ItemData>
                {
                    new ItemData($"Order {order.Id} - {order.Description}", 1, (int)order.TotalPrice)
                };

                var paymentData = new PaymentData(
                    orderCode: orderCode,
                    amount: (int)order.TotalPrice,
                    description: $"DH-{order.Id.ToString().Substring(0, 8)}", // Giới hạn ký tự
                    items: items,
                    cancelUrl: _configuration["PayOS:CancelUrl"],
                    returnUrl: _configuration["PayOS:ReturnUrl"]
                );

                var paymentLink = await _payOS.createPaymentLink(paymentData);
                if (paymentLink == null || string.IsNullOrEmpty(paymentLink.checkoutUrl))
                {
                    _logger.LogError("Không tạo được link thanh toán cho OrderId: {OrderId}", orderId);
                    return BadRequest(new { message = "Lỗi tạo liên kết thanh toán." });
                }

                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    OrderCode = orderCode,
                    Amount = order.TotalPrice,
                    Description = paymentData.description,
                    PaymentLink = paymentLink.checkoutUrl,
                    ReturnUrl = paymentData.returnUrl,
                    CancelUrl = paymentData.cancelUrl,
                    Status = 0, // Trạng thái mặc định: Pending
                    CreatedAt = DateTime.UtcNow
                };

                await _paymentService.CreatePaymentAsync(payment);

                _logger.LogInformation("Tạo thanh toán thành công cho OrderId: {OrderId}, OrderCode: {OrderCode}", orderId, orderCode);

                return Ok(new
                {
                    orderId = order.Id,
                    orderCode = orderCode,
                    checkoutUrl = paymentLink.checkoutUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo thanh toán cho OrderId: {OrderId}", orderId);
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }
    }
}

