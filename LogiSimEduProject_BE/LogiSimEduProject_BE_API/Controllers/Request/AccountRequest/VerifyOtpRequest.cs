﻿namespace LogiSimEduProject_BE_API.Controllers.Request.AccountRequest
{
    public class VerifyOtpRequest
    {
        public string Email { get; set; }
        public string Otp { get; set; }
    }

}
