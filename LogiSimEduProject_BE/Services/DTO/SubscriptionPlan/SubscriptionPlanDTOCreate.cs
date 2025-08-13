using System.ComponentModel.DataAnnotations;

namespace Services.DTO.SubscriptionPlan
{
    public class SubscriptionPlanDTOCreate
    {
        [Required]
        [StringLength(50, ErrorMessage = "Name tối đa 50 ký tự.")]
        public string? Name { get; set; }
        [Required]
        [Range(0, 1_000_000, ErrorMessage = "Price phải ≥ 0.")]
        public decimal? Price { get; set; }
        [Required]
        [Range(1, 120, ErrorMessage = "DurationInMonths phải từ 1–120.")]
        public int? DurationInMonths { get; set; }
        [Required]

        [Range(1, 10_000, ErrorMessage = "MaxWorkSpaces phải ≥ 1.")]
        public int? MaxWorkSpaces { get; set; }
        [Required]
        [StringLength(1000, ErrorMessage = "Description tối đa 1000 ký tự.")]
        public string? Description { get; set; }
        //public bool? IsActive { get; set; }
    }
}
