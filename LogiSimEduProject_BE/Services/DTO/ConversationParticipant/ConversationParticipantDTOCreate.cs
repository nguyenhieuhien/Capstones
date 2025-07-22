namespace Services.DTO.ConversationParticipant
{
    public class ConversationParticipantDTOCreate
    {
        public Guid ConversationId { get; set; }

        public Guid AccountId { get; set; }

    }
}
