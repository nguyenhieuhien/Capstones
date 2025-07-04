namespace LogiSimEduProject_BE_API.Controllers.DTO.SubmitQuiz
{
    public class QuizAnswerSubmission
    {
        public Guid QuestionId { get; set; }
        public Guid AnswerId { get; set; }
    }
}
