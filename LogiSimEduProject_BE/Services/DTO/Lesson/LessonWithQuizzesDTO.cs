using Services.DTO.LessonProgress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO.Lesson
{
    public class LessonWithQuizzesDTO
    {
        public Guid Id { get; set; }
        public Guid? TopicId { get; set; }

        public string LessonName { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
        public List<QuizWithLatestScoreDto> Quizzes { get; set; } = new();
        public ScenarioDto Scenario { get; set; }  // có thể null
        public List<LessonProgressDto> LessonProgresses { get; set; } = new();
        public List<LessonSubmissionDto> LessonSubmissions { get; set; } = new();
    }

    public sealed class QuizWithLatestScoreDto
    {
        public Guid Id { get; set; }
        public string QuizName { get; set; } = "";
        public double? TotalScore { get; set; }
        public double? LatestScore { get; set; } // điểm nộp gần nhất của account (nếu truyền accountId)
    }

    public sealed class ScenarioDto
    {
        public Guid Id { get; set; }
        public string ScenarioName { get; set; }
        public string FileUrl { get; set; }
        public string Description { get; set; }
    }

    public sealed class LessonProgressDto
    {
        public Guid Id { get; set; }
        public Guid? AccountId { get; set; }
        public int? Status { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public sealed class LessonSubmissionDto
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public DateTime SubmitTime { get; set; }
        public string FileUrl { get; set; }
        public string Note { get; set; }
        public double? TotalScore { get; set; }
    }
}
