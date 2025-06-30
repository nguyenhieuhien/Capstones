namespace LogiSimEduProject_BE_API.Controllers.DTO.Account
{
    public class ChangePasswordRequest
    {  
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

}
