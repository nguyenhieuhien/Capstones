namespace LogiSimEduProject_BE_API.Controllers.DTO.Notification
{
    public class NotificationDTOUpdate
    {
        public Guid AccountId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
    }
}
