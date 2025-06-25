using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnswerController : ControllerBase
    {
        private readonly IAnswerService _service;
        public AnswerController(IAnswerService service) => _service = service;
        [HttpGet]
        public async Task<IEnumerable<Answer>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<Answer> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1")]
        [HttpPost]
        public async Task<int> Post(Answer answer)
        {
            return await _service.Create(answer);
        }

        //[Authorize(Roles = "1")]
        [HttpPut("{id}")]
        public async Task<int> Put(Answer answer)
        {
            return await _service.Update(answer);
        }

        //[Authorize(Roles = "1")]
        [HttpDelete("{id}")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
