using Repositories.Models;

namespace LogiSimEduProject_BE_API.Controllers.DTO
{
    public class AccountDTO
    {
        public Guid Id { get; set; }

        public Guid RoleId { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public bool? IsActive { get; set; }

    }
}
