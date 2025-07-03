using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("LoginGoogle")]
    public async Task<IActionResult> GoogleLogin([FromBody] TokenRequest request)
    {
        try
        {
            // Xác minh ID token từ Firebase
            FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.Token);
            string uid = decodedToken.Uid;

            string email = decodedToken.Claims.ContainsKey("email")
                ? decodedToken.Claims["email"]?.ToString()
                : null;

            return Ok(new
            {
                message = "Login successful",
                uid,
                email
            });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = "Invalid token", error = ex.Message });
        }
    }

    public class TokenRequest
    {
        public string Token { get; set; }
    }
}
