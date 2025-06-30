namespace LogiSimEduProject_BE_API.Controllers.DTO.Answer
{
    public class AnswerDTOUpdate
    {
        public Guid QuestionId { get; set; }
        public string Description { get; set; }
        public bool IsAnswerCorrect { get; set; }
    }
}
