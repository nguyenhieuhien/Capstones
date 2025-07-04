using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;

namespace LogiSimEduProject_BE_API.Controllers
{
    public class PaymentController : ControllerBase
    {
        private readonly PayOS _payOS;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IConfiguration configuration, ILogger<PaymentController> logger)
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
            _logger = logger;
        }

        //[HttpPost("create-payment/{bookingId}")]
        //public async Task<IActionResult> CreatePayment(long bookingId)
        //{
        //    try
        //    {
        //        var booking = await _bookingService.GetBookingById(bookingId);
        //        if (booking == null)
        //        {
        //            _logger.LogWarning("Booking not found for ID: {BookingId}", bookingId);
        //            return NotFound(new { message = "Không tìm thấy đơn đặt." });
        //        }

        //        if (booking.TotalPrice <= 0)
        //        {
        //            _logger.LogWarning("Invalid TotalPrice for booking ID: {BookingId}, TotalPrice: {TotalPrice}", bookingId, booking.TotalPrice);
        //            return BadRequest(new { message = "Giá trị thanh toán phải lớn hơn 0." });
        //        }

        //        var items = new List<ItemData>
        //        {
        //            new ItemData($"Booking {booking.Id} - {booking.Description}", 1, (int)booking.TotalPrice)
        //        };

        //        var customOrderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        //        var paymentData = new PaymentData(
        //            orderCode: customOrderCode,
        //            amount: (int)booking.TotalPrice,
        //            description: $"Thanh toán Booking {booking.Id}",
        //            items: items,
        //            cancelUrl: _configuration["PayOS:CancelUrl"],
        //            returnUrl: _configuration["PayOS:ReturnUrl"]
        //        );

        //        var createPayment = await _payOS.createPaymentLink(paymentData);
        //        if (createPayment == null || string.IsNullOrEmpty(createPayment.checkoutUrl))
        //        {
        //            _logger.LogError("Failed to create payment link for booking ID: {BookingId}", bookingId);
        //            return BadRequest(new { message = "Lỗi khi tạo link thanh toán" });
        //        }

        //        var payment = new Models.Payment
        //        {
        //            BookingId = booking.Id,
        //            OrderCode = customOrderCode,
        //            Amount = (int)booking.TotalPrice,
        //            Description = paymentData.description,
        //            Items = $"Booking {booking.Id} - {booking.Description}",
        //            CancelUrl = paymentData.cancelUrl,
        //            ReturnUrl = paymentData.returnUrl,
        //            PaymentLink = createPayment.checkoutUrl,
        //            Status = "PENDING",
        //            CreatedAt = DateTime.UtcNow
        //        };

        //        await _paymentService.CreatePaymentAsync(payment);
        //        _logger.LogInformation("Payment created successfully for booking ID: {BookingId}, OrderCode: {OrderCode}", bookingId, customOrderCode);

        //        return Ok(new
        //        {
        //            bookingId = booking.Id,
        //            orderCode = customOrderCode,
        //            checkoutUrl = createPayment.checkoutUrl
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error creating payment for booking ID: {BookingId}", bookingId);
        //        return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
        //    }
        //}


        //[HttpPut("update")]
        //public async Task<IActionResult> ConfirmPayOsTransaction([FromBody] PaymentDTO paymentDTO)
        //{
        //    try
        //    {
        //        // Kiểm tra trạng thái thanh toán từ PayOS
        //        var transactionInfo = await _payOS.getPaymentLinkInformation(paymentDTO.OrderCode);
        //        var payment = await _paymentService.GetByOrderCodeAsync(paymentDTO.OrderCode);

        //        if (payment == null)
        //        {
        //            _logger.LogWarning("Payment not found for OrderCode: {OrderCode}", paymentDTO.OrderCode);
        //            return NotFound(new { message = "Không tìm thấy thanh toán." });
        //        }

        //        // Lấy BookingDTO liên quan
        //        var bookingDTO = await _bookingService.GetBookingById(payment.BookingId);
        //        if (bookingDTO == null)
        //        {
        //            _logger.LogWarning("Booking not found for ID: {BookingId}", payment.BookingId);
        //            return NotFound(new { message = "Không tìm thấy đơn đặt." });
        //        }

        //        string newBookingStatus = bookingDTO.Status;

        //        if (transactionInfo.status == "CANCELLED")
        //        {
        //            payment.Status = "CANCELLED";
        //            newBookingStatus = "CANCELLED";
        //            await _paymentService.UpdatePaymentAsync(payment);
        //            await _bookingService.UpdateBookingStatusAsync(payment.BookingId, newBookingStatus);
        //            _logger.LogInformation("Payment and Booking status updated to CANCELLED for OrderCode: {OrderCode}", paymentDTO.OrderCode);
        //            return Ok(new { message = "Đã cập nhật trạng thái thanh toán và đơn đặt thành hủy." });
        //        }
        //        else if (transactionInfo.status == "PAID")
        //        {
        //            payment.Status = "PAID";
        //            newBookingStatus = "Confirmed"; // Giả sử trạng thái Booking khi thanh toán thành công là COMPLETED
        //            await _paymentService.UpdatePaymentAsync(payment);
        //            await _bookingService.UpdateBookingStatusAsync(payment.BookingId, newBookingStatus);
        //            _logger.LogInformation("Payment and Booking status updated to PAID/COMPLETED for OrderCode: {OrderCode}", paymentDTO.OrderCode);
        //            return Ok(new { message = "Đã cập nhật trạng thái thanh toán và đơn đặt thành công." });
        //        }
        //        else
        //        {
        //            _logger.LogWarning("Invalid transaction status: {Status} for OrderCode: {OrderCode}", transactionInfo.status, paymentDTO.OrderCode);
        //            return BadRequest(new { message = "Trạng thái giao dịch không hợp lệ." });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error updating payment and booking status for OrderCode: {OrderCode}", paymentDTO.OrderCode);
        //        return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
        //    }
        //}
    }
}
