using LogiSimEduProject_BE_API.Controllers.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;
using Repositories.Models;
using Services;
using Services.IServices;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[Route("api/payment")]
public class PaymentController : ControllerBase
{
    private readonly PayOS _payOS;
    private readonly IOrderService _orderService;
    private readonly IOrganizationService _organizationService;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger;
    private readonly IConfiguration _configuration;

    public PaymentController(IConfiguration configuration, IOrderService orderService, IPaymentService paymentService, IOrganizationService organizationService, ILogger<PaymentController> logger)
    {
        _configuration = configuration;
        string clientId = configuration["PayOS:ClientId"];
        string apiKey = configuration["PayOS:ApiKey"];
        string checksumKey = configuration["PayOS:ChecksumKey"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(checksumKey))
        {
            throw new ArgumentException("PayOS configuration is missing or invalid.");
        }

        _organizationService = organizationService;
        _payOS = new PayOS(clientId, apiKey, checksumKey);
        _orderService = orderService;
        _paymentService = paymentService;
        _logger = logger;
    }

    // ===================== GET: All Payments =====================
    [Authorize(Roles = "Organization_Admin")]
    [HttpGet("get_all_payment")]
    [SwaggerOperation(Summary = "Lấy danh sách tất cả thanh toán (mới nhất trước)")]
    public async Task<IActionResult> GetAllPayments()
    {
        try
        {
            var payments = await _paymentService.GetAllAsync();

            // Sắp xếp mới nhất trước (nếu cần)
            var result = payments
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    p.Id,
                    p.OrderId,
                    p.OrderCode,
                    p.Amount,
                    p.Description,
                    p.PaymentLink,
                    p.ReturnUrl,
                    p.CancelUrl,
                    p.Status,
                    p.CreatedAt,
                });

            _logger.LogInformation("Lấy {Count} payment(s) thành công.", result.Count());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách thanh toán.");
            return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
        }
    }

    // ===================== GET: Payment by Id =====================
    [Authorize(Roles = "Organization_Admin")]
    [HttpGet("get_payment{id}")]
    [SwaggerOperation(Summary = "Lấy thông tin thanh toán theo Id")]
    public async Task<IActionResult> GetPaymentById(Guid id)
    {
        try
        {
            var payment = await _paymentService.GetByIdAsync(id);
            if (payment == null)
            {
                _logger.LogWarning("Không tìm thấy thanh toán với Id: {Id}", id);
                return NotFound(new { message = "Không tìm thấy thanh toán." });
            }

            var result = new
            {
                payment.Id,
                payment.OrderId,
                payment.OrderCode,
                payment.Amount,
                payment.Description,
                payment.PaymentLink,
                payment.ReturnUrl,
                payment.CancelUrl,
                payment.Status,
                payment.CreatedAt,
            };

            _logger.LogInformation("Lấy thanh toán thành công. Id: {Id}", id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thanh toán Id: {Id}", id);
            return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
        }
    }





    [Authorize(Roles = "Organization_Admin")]
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
                description: $"DH-{order.Id.ToString().Substring(0, 8)}",
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
                Status = 0,
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


    [HttpPut("update")]
    [SwaggerOperation(Summary = "Xác nhận trạng thái giao dịch từ PayOS và cập nhật hệ thống")]
    public async Task<IActionResult> ConfirmPayOsTransaction([FromBody] PaymentDTO paymentDTO)
    {
        try
        {
            // 1) Lấy trạng thái đơn trên PayOS (tránh trust dữ liệu client)
            var transactionInfo = await _payOS.getPaymentLinkInformation(paymentDTO.OrderCode);

            // 2) Tìm payment trong hệ thống
            var payment = await _paymentService.GetByOrderCodeAsync(paymentDTO.OrderCode);
            if (payment == null)
            {
                _logger.LogWarning("Không tìm thấy thanh toán với OrderCode: {OrderCode}", paymentDTO.OrderCode);
                return NotFound(new { message = "Không tìm thấy thanh toán." });
            }

            // (2b) Tìm order để nắm OrganizationId
            if (!payment.OrderId.HasValue)
            {
                _logger.LogError("Payment không có OrderId. OrderCode: {OrderCode}", paymentDTO.OrderCode);
                return StatusCode(500, new { message = "Dữ liệu thanh toán không hợp lệ (thiếu OrderId)." });
            }

            var order = await _orderService.GetByIdAsync(payment.OrderId.Value);
            if (order == null)
            {
                _logger.LogWarning("Không tìm thấy Order với Id: {OrderId}", payment.OrderId);
                return NotFound(new { message = "Không tìm thấy đơn hàng tương ứng với thanh toán." });
            }

            // 3) Xác định trạng thái mới
            int newPaymentStatus;
            int? newOrderStatus = null;
            bool shouldActivateOrganization = false;

            if (string.Equals(transactionInfo.status, "PAID", StringComparison.OrdinalIgnoreCase))
            {
                newPaymentStatus = 1;   // PAID
                newOrderStatus = 1;     // CONFIRMED
                shouldActivateOrganization = true; // <-- cần kích hoạt org
            }
            else if (string.Equals(transactionInfo.status, "CANCELLED", StringComparison.OrdinalIgnoreCase))
            {
                newPaymentStatus = 2;   // CANCELLED
                newOrderStatus = 2;     // CANCELLED
                shouldActivateOrganization = false; // chỉ kích hoạt khi PAID
            }
            else
            {
                return BadRequest(new { message = $"Trạng thái giao dịch không hợp lệ: {transactionInfo.status}" });
            }

            // 4) Idempotency: nếu trạng thái không đổi thì vẫn đảm bảo Order & Organization đúng
            if ((payment.Status ?? 0) == newPaymentStatus)
            {
                if (newOrderStatus.HasValue)
                    await _orderService.UpdateStatusAsync(order.Id, newOrderStatus.Value);

                if (shouldActivateOrganization && order.OrganizationId.HasValue)
                {
                    var orgUpdated = await _organizationService.UpdateActiveAsync(order.OrganizationId.Value, true);
                    if (!orgUpdated)
                    {
                        _logger.LogError("Cập nhật Organization active thất bại. OrgId: {OrgId}", order.OrganizationId);
                        return StatusCode(500, new { message = "Cập nhật tổ chức thất bại." });
                    }
                }

                _logger.LogInformation("Bỏ qua cập nhật vì trạng thái thanh toán đã là {Status} cho OrderCode: {OrderCode}",
                    newPaymentStatus, paymentDTO.OrderCode);

                return Ok(new
                {
                    message = "Trạng thái đã được cập nhật trước đó.",
                    paymentStatus = newPaymentStatus,
                    orderStatus = newOrderStatus,
                    organizationActivated = shouldActivateOrganization
                });
            }

            // 5) Cập nhật Payment
            payment.Status = newPaymentStatus;
            await _paymentService.UpdatePaymentAsync(payment);

            // 6) Cập nhật Order
            if (newOrderStatus.HasValue)
            {
                var orderUpdated = await _orderService.UpdateStatusAsync(order.Id, newOrderStatus.Value);
                if (!orderUpdated)
                {
                    _logger.LogError("Cập nhật Order thất bại cho OrderId: {OrderId}", order.Id);
                    return StatusCode(500, new { message = "Cập nhật đơn hàng thất bại." });
                }
            }

            // 7) Kích hoạt Organization nếu PAID
            if (shouldActivateOrganization && order.OrganizationId.HasValue)
            {
                var orgUpdated = await _organizationService.UpdateActiveAsync(order.OrganizationId.Value, true);
                if (!orgUpdated)
                {
                    _logger.LogError("Cập nhật Organization active thất bại. OrgId: {OrgId}", order.OrganizationId);
                    return StatusCode(500, new { message = "Cập nhật tổ chức thất bại." });
                }
            }

            _logger.LogInformation(
                "Đã cập nhật Payment.Status={PaymentStatus}, Order.Status={OrderStatus}, OrgActivated={OrgActivated} cho OrderCode: {OrderCode}",
                newPaymentStatus, newOrderStatus, shouldActivateOrganization, paymentDTO.OrderCode);

            return Ok(new
            {
                message = "Cập nhật trạng thái thanh toán, đơn hàng và tổ chức thành công.",
                paymentStatus = newPaymentStatus,
                orderStatus = newOrderStatus,
                organizationActivated = shouldActivateOrganization
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xác nhận thanh toán cho OrderCode: {OrderCode}", paymentDTO.OrderCode);
            return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
        }
    }
}





