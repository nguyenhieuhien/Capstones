using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO.Topic
{
    public class TopicWithFinishDTO
    {
        public Guid Id { get; set; }
        public Guid? CourseId { get; set; }
        public string TopicName { get; set; }
        public int OrderIndex { get; set; }
        public string Description { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeleteAt { get; set; }

        // NEW: danh sách AccountId hoàn thành toàn bộ lessons của topic
        public List<Guid> StudentFinish { get; set; } = new();

    }
}
