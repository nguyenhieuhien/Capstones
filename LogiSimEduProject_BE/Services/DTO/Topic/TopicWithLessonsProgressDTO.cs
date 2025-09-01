using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO.Topic
{
    public class TopicWithLessonsProgressDTO
    {
        public Guid TopicId { get; set; }
        public string TopicName { get; set; }
        public int OrderIndex { get; set; }
        public List<LessonWithProgressDto> Lessons { get; set; } = new();
    }

    public sealed class LessonProgressDto
    {
        public Guid Id { get; set; }
        public Guid? AccountId { get; set; }
        public Guid? LessonId { get; set; }
        public int? Status { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public sealed class LessonWithProgressDto
    {
        public Guid LessonId { get; set; }
        public string LessonName { get; set; }
        public int OrderIndex { get; set; }
        public List<LessonProgressDto> Progresses { get; set; } = new();
    }
}
