namespace LogiSimEduProject_BE_API.Controllers.DTO.Package
{
    public class PackageDTOUpdate
    {
        public Guid OrderId { get; set; }

        public Guid WorkSpaceId { get; set; }

        public Guid PackageTypeId { get; set; }
    }
}
