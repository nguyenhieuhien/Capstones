namespace LogiSimEduProject_BE_API.Controllers.DTO.Scene
{
    public class SceneDTOCreate
    {
        public string SceneName { get; set; }

        public string Description { get; set; }

        public IFormFile ImgUrl { get; set; }
    }
}
