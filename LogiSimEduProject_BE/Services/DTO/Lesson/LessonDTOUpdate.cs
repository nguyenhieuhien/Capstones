using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO.Lesson
{
    public class LessonDTOUpdate
    {
        public Guid? TopicId { get; set; }

        public string LessonName { get; set; }

        public int OrderIndex { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int? Status { get; set; }
    }
}
