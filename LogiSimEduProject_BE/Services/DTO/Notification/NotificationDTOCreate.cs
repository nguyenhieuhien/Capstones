namespace Services.DTO.Notification
{
    public class NotificationDTOCreate
    {
        public Guid AccountId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
    }
}
