namespace LogiSimEduProject_BE_API.Controllers.Request.AccountRequest
{
    public sealed record RegisterRequest(string UserName, string FullName, string Email, string Password, string Phone);
}
