namespace Services.DTO.SubscriptionPlan
{
    public class SubscriptionPlanDTOUpdate
    {
        public string ?Name { get; set; }
        public double? Price { get; set; }
        public int? DurationInMonths { get; set; }
        public int? MaxWorkSpaces { get; set; }
        public string ?Description { get; set; }
        public bool? IsActive { get; set; }
    }
}
