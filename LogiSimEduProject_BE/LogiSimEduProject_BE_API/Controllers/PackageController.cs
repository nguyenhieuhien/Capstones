//using LogiSimEduProject_BE_API.Controllers.DTO.Package;
//using LogiSimEduProject_BE_API.Controllers.DTO.Question;
//using Microsoft.AspNetCore.Mvc;
//using Repositories.Models;
//using Services;

//// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

//namespace LogiSimEduProject_BE_API.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class PackageController : ControllerBase
//    {
//        private readonly IPackageService _service;
//        public PackageController(IPackageService service) => _service = service;
//        [HttpGet("GetAllPackage")]
//        public async Task<IEnumerable<Package>> Get()
//        {
//            return await _service.GetAll();
//        }

//        [HttpGet("GetPackage/{id}")]
//        public async Task<Package> Get(string id)
//        {
//            return await _service.GetById(id);
//        }

//        //[Authorize(Roles = "1")]
//        [HttpPost("CreatePackage")]
//        public async Task<IActionResult> Post(PackageDTOCreate request)
//        {
//            var question = new Question
//            {
//                QuizId = request.OrderId,
//                Description = request.WorkSpaceId,
//                IsCorrect = request.,
//                IsActive = true,
//                CreatedAt = DateTime.UtcNow
//            };
//            var result = await _service.Create(question);

//            if (result <= 0)
//                return BadRequest("Fail Create");

//            return Ok(new
//            {
//                Data = request
//            });
//        }

//        //[Authorize(Roles = "1")]
//        [HttpPut("UpdatePackage/{id}")]
//        public async Task<IActionResult> Put(string id, QuestionDTOUpdate request)
//        {
//            var existingQuestion = await _service.GetById(id);
//            if (existingQuestion == null)
//            {
//                return NotFound(new { Message = $"Question with ID {id} was not found." });
//            }

//            existingQuestion.QuizId = request.QuizId;
//            existingQuestion.Description = request.Description;
//            existingQuestion.IsCorrect = request.IsCorrect;
//            existingQuestion.UpdatedAt = DateTime.UtcNow;

//            await _service.Update(existingQuestion);

//            return Ok(new
//            {
//                Message = "Question updated successfully.",
//                Data = new
//                {
//                    QuizId = existingQuestion.QuizId,
//                    Description = existingQuestion.Description,
//                    IsCorrect = existingQuestion.IsCorrect,
//                    IsActive = existingQuestion.IsActive,
//                }
//            });
//        }

//        //[Authorize(Roles = "1")]
//        [HttpDelete("DeletePackage/{id}")]
//        public async Task<bool> Delete(string id)
//        {
//            return await _service.Delete(id);
//        }
//    }
//}
