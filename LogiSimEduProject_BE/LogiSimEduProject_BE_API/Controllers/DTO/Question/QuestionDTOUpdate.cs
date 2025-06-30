namespace LogiSimEduProject_BE_API.Controllers.DTO.Question
{
    public class QuestionDTOUpdate
    {
        public Guid QuizId { get; set; }
        public string Description { get; set; }
        public bool? IsCorrect { get; set; }
    }
}
