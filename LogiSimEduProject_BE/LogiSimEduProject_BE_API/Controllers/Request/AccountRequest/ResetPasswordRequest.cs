namespace LogiSimEduProject_BE_API.Controllers.Request.AccountRequest
{
    public class ResetPasswordRequest
    {
        public required string Token { get; set; }
        public required string NewPassword { get; set; }
    }
}
