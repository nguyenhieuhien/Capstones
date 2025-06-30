namespace LogiSimEduProject_BE_API.Controllers.DTO.Account
{
    public class QuizDTOUpdate
    {
        public Guid TopicId { get; set; }
        public string QuizName { get; set; }
        public double? Score { get; set; }
        public string Status { get; set; }
    }
}
