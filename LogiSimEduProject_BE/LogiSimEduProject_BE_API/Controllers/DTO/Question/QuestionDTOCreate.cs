namespace LogiSimEduProject_BE_API.Controllers.DTO.Questions
{
    public class QuestionDTOCreate
    {
        public Guid QuizId { get; set; }
        public string Description { get; set; }
        public bool? IsCorrect { get; set; }
    }
}
