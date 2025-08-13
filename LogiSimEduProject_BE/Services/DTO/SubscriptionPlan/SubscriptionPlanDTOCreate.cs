using System.ComponentModel.DataAnnotations;

namespace Services.DTO.SubscriptionPlan
{
    public class SubscriptionPlanDTOCreate
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public double? Price { get; set; }
        [Required]
        public int? DurationInMonths { get; set; }
        [Required]
        public int? MaxWorkSpaces { get; set; }
        [Required]
        public string Description { get; set; }
        public bool? IsActive { get; set; }
    }
}
