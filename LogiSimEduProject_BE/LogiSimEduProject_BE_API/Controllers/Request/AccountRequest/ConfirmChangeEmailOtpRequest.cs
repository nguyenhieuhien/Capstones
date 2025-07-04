namespace LogiSimEduProject_BE_API.Controllers.Request.AccountRequest
{
    public class ConfirmChangeEmailOtpRequest
    {
        public string NewEmail { get; set; }
        public string Otp { get; set; }
    }

}
