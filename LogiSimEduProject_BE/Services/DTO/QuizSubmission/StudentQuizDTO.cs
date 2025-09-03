using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO.QuizSubmission
{
    public class StudentQuizDTO
    {
        public Guid? AccountId { get; set; }
        public string FullName { get; set; }
        public Guid? QuizId { get; set; }
        public string QuizName { get; set; }
        public DateTime? SubmitTime { get; set; }
        public double? TotalScore { get; set; }
    }

    public class QuizResultByClassDto
    {
        public string ClassName { get; set; }
        public List<StudentQuizDTO> Students { get; set; }
    }
}
