using LogiSimEduProject_BE_API.Controllers.DTO.Package;
using LogiSimEduProject_BE_API.Controllers.DTO.Question;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackageController : ControllerBase
    {
        private readonly IPackageService _service;
        public PackageController(IPackageService service) => _service = service;
        [HttpGet("GetAllPackage")]
        public async Task<IEnumerable<Package>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("GetPackage/{id}")]
        public async Task<Package> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1")]
        [HttpPost("CreatePackage")]
        public async Task<IActionResult> Post(PackageDTOCreate request)
        {
            var question = new Package
            {
                OrderId = request.OrderId,
                WorkSpaceId = request.WorkSpaceId,
                PackageTypeId = request.PackageTypeId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _service.Create(question);

            if (result <= 0)
                return BadRequest("Fail Create");

            return Ok(new
            {
                Data = request
            });
        }

        //[Authorize(Roles = "1")]
        [HttpPut("UpdatePackage/{id}")]
        public async Task<IActionResult> Put(string id, PackageDTOUpdate request)
        {
            var existingPackage = await _service.GetById(id);
            if (existingPackage == null)
            {
                return NotFound(new { Message = $"Package with ID {id} was not found." });
            }

            existingPackage.OrderId = request.OrderId;
            existingPackage.WorkSpaceId = request.WorkSpaceId;
            existingPackage.PackageTypeId = request.PackageTypeId;
            existingPackage.UpdatedAt = DateTime.UtcNow;

            await _service.Update(existingPackage);

            return Ok(new
            {
                Message = "Package updated successfully.",
                Data = new
                {
                    OrderId = request.OrderId,
                    //QuizId = existingQuestion.QuizId,
                    //Description = existingQuestion.Description,
                    //IsCorrect = existingQuestion.IsCorrect,
                    //IsActive = existingQuestion.IsActive,
                }
            });
        }

        //[Authorize(Roles = "1")]
        [HttpDelete("DeletePackage/{id}")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
