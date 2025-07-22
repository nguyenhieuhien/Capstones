using Microsoft.AspNetCore.Http;

namespace Services.DTO.Scene
{
    public class SceneDTOCreate
    {
        public string SceneName { get; set; }

        public string Description { get; set; }
    }
}
