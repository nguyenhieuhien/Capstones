namespace LogiSimEduProject_BE_API.Controllers.Request.AccountRequest
{
    public class ChangeEmailRequest
    {
        public required string NewEmail { get; set; }
        public required string Password { get; set; }
    }
}
