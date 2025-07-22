namespace Services.DTO.QuizSubmission
{
    public class QuizSubmissionRequest
    {
        public Guid QuizId { get; set; }
        public Guid AccountId { get; set; }
        public List<QuizAnswerPair> Answers { get; set; } = new();
    }
}
