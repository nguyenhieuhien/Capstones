namespace LogiSimEduProject_BE_API.Controllers.DTO.Workspace
{
    public class WorkspaceDTOCreate
    {
        public Guid? OrderId { get; set; }

        public Guid? OrganizationId { get; set; }

        public string WorkSpaceName { get; set; }

        public int? NumberOfAccount { get; set; }

        public string ImgUrl { get; set; }

        public string Description { get; set; }
    }
}

