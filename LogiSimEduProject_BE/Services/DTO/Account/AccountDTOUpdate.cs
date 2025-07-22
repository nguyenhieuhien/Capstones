namespace Services.DTO.Account
{
    public class AccountDTOUpdate
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public Guid OrganizationId { get; set; }
        public int RoleId { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
    }
}
