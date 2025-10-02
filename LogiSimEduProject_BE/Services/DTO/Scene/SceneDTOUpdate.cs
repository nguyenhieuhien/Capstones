using Microsoft.AspNetCore.Http;

namespace Services.DTO.Scene
{
    public class SceneDTOUpdate
    {
        public Guid InstructorId { get; set; }
        public string SceneName { get; set; }

        public string Description { get; set; }
    }
}
