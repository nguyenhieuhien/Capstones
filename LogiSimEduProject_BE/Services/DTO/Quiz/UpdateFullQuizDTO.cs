using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO.Quiz
{
    public class UpdateFullQuizDTO
    {
        public string QuizName { get; set; } = string.Empty;

        public List<UpdateQuestionDto> Questions { get; set; } = new();
    }

    public class UpdateQuestionDto
    {
        public Guid? Id { get; set; } // Nếu Guid.Empty thì hiểu là thêm mới
        public string Description { get; set; } 

        public List<UpdateAnswerDto> Answers { get; set; } = new();
    }

    public class UpdateAnswerDto
    {
        public Guid? Id { get; set; } // Nếu Guid.Empty thì hiểu là thêm mới
        public string Description { get; set; }
        public bool IsCorrect { get; set; }
    }
}
