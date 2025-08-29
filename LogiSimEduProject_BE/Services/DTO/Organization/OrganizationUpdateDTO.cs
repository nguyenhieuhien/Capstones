using System.ComponentModel.DataAnnotations;

namespace Services.DTO.Organization
{
    public class OrganizationUpdateDTO
    {
        [StringLength(150, ErrorMessage = "OrganizationName tối đa 150 ký tự.")]
        public string? OrganizationName { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(150, ErrorMessage = "Email tối đa 150 ký tự.")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(20, ErrorMessage = "Phone tối đa 20 ký tự.")]
        public string? Phone { get; set; }

        [StringLength(250, ErrorMessage = "Address tối đa 250 ký tự.")]
        public string? Address { get; set; }

        public string? ImgUrl { get; set; }

        public bool? IsActive { get; set; }
    }
}
