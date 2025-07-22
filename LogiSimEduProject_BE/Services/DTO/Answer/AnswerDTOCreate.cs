namespace Services.DTO.Answer
{
    public class AnswerDTOCreate
    {
        public Guid QuestionId { get; set; }
        public string Description { get; set; }
        public bool IsCorrect { get; set; }
    }
}
