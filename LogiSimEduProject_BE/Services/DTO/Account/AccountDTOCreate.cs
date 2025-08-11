namespace Services.DTO.Account
{
    public class AccountDTOCreate
    {
        public Guid OrganizationId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public int? Gender { get; set; }
        public string Address { get; set; }
    }
}
