namespace LogiSimEduProject_BE_API.Controllers.Request
{
    public sealed class UploadResultRequest
    {
        public string StudentId { get; set; } = "";
        public IFormFile File { get; set; } = default!;
    }
}
