using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Services.DTO.Lesson
{
    public class LessonDTOCreate
    {
        public Guid? TopicId { get; set; }

        public Guid? ScenarioId { get; set; }

        public string LessonName { get; set; }

        public int OrderIndex { get; set; }

        [JsonIgnore]
        public IFormFile? FileUrl { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
    }
}
