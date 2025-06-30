namespace LogiSimEduProject_BE_API.Controllers.DTO.ConversationParticipant
{
    public class ConversationParticipantDTOCreate
    {
        public Guid ConversationId { get; set; }

        public Guid AccountId { get; set; }

    }
}
