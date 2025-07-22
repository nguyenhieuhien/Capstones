using Microsoft.AspNetCore.Http;

namespace Services.Controllers.DTO.Workspace
{
    public class WorkspaceDTOUpdate
    {
        public Guid? OrderId { get; set; }

        public Guid? OrganizationId { get; set; }

        public string WorkSpaceName { get; set; }

        public int? NumberOfAccount { get; set; }

        public IFormFile ImgUrl { get; set; }

        public string Description { get; set; }
    }
}
