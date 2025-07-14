namespace LogiSimEduProject_BE_API.Controllers.DTO.Account
{
    public class AccountDTOUpdate
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public Guid OrganizationId { get; set; }
        public bool? SystemMode { get; set; }
        public int OrganizationRoleId { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
    }
}
