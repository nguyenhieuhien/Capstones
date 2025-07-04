//using Microsoft.AspNetCore.Mvc;

//[ApiController]
//[Route("api/[controller]")]
//public class TestEmailController : ControllerBase
//{
//    private readonly EmailService _emailService;

//    public TestEmailController(EmailService emailService)
//    {
//        _emailService = emailService;
//    }

//    [HttpGet("SendTestEmail")]
//    public async Task<IActionResult> SendTestEmail()
//    {
//        await _emailService.SendEmailAsync(
//            "karsa0097@gmail.com",
//            "Test gửi mail DI",
//            "<h3>Email gửi từ hệ thống qua DI thành công!</h3>"
//        );

//        return Ok("Đã gửi email.");
//    }
//}
