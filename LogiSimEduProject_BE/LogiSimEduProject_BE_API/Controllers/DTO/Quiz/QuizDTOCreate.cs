namespace LogiSimEduProject_BE_API.Controllers.DTO.Quiz
{
    public class QuizDTOCreate
    {
        public Guid TopicId { get; set; }
        public string QuizName { get; set; }
        public double? Score { get; set; }
        //public string Status { get; set; }

    }
}
