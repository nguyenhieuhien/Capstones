using System.ComponentModel.DataAnnotations;

namespace Services.DTO.Organization
{
    public class OrganizationCreateDTO
    {
        [Required(ErrorMessage = "OrganizationName là bắt buộc.")]
        [StringLength(150, ErrorMessage = "OrganizationName tối đa 150 ký tự.")]
        public string OrganizationName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(150, ErrorMessage = "Email tối đa 150 ký tự.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone là bắt buộc.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(20, ErrorMessage = "Phone tối đa 20 ký tự.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Address là bắt buộc.")]
        [StringLength(250, ErrorMessage = "Address tối đa 250 ký tự.")]
        public string Address { get; set; }
    }
}
