namespace LogiSimEduProject_BE_API.Controllers.DTO.SubmitQuiz
{
    public class QuizSubmissionRequest
    {
        public Guid QuizId { get; set; }
        public Guid AccountId { get; set; }
        public List<QuizAnswerSubmission> Answers { get; set; } = new();
    }
}
