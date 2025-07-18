namespace LogiSimEduProject_BE_API.Controllers.DTO.Answer
{
    public class AnswerDTOCreate
    {
        public Guid QuestionId { get; set; }
        public string Description { get; set; }
        public bool IsCorrect { get; set; }
    }
}
