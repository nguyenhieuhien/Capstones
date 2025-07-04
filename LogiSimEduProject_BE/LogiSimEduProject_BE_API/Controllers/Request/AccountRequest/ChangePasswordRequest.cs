namespace LogiSimEduProject_BE_API.Controllers.Request.AccountRequest
{
    public class ChangePasswordRequest
    {  
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

}
