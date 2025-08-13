using System.ComponentModel.DataAnnotations;

namespace Services.DTO.Account
{
    public class AccountDTOCreate
    {
        [Required(ErrorMessage = "OrganizationId là bắt buộc.")]
        public Guid OrganizationId { get; set; }

        [Required(ErrorMessage = "UserName là bắt buộc.")]
        [StringLength(20, ErrorMessage = "UserName tối đa 20 ký tự.")]
        [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "UserName chỉ gồm chữ, số, ., _, -")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "FullName là bắt buộc.")]
        [StringLength(50, ErrorMessage = "FullName tối đa 50 ký tự.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(50, ErrorMessage = "Email tối đa 50 ký tự.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password là bắt buộc.")]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "Password tối thiểu 8 ký tự.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "Password cần ít nhất 1 chữ thường, 1 chữ hoa và 1 chữ số.")]
        public string Password { get; set; }
        //public int? Gender { get; set; }
        //public string Address { get; set; }
    }
}
