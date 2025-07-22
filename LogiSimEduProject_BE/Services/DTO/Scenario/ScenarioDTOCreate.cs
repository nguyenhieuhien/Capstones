using Microsoft.AspNetCore.Http;

namespace Services.DTO.Scenario
{
    public class ScenarioDTOCreate
    {
        public Guid SceneId { get; set; }

        public string ScenarioName { get; set; }

        public string Description { get; set; }

        public IFormFile FileUrl { get; set; }
    }
}
