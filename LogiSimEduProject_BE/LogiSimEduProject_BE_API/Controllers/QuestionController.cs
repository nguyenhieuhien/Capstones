using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _service;
        public QuestionController(IQuestionService service) => _service = service;
        [HttpGet]
        public async Task<IEnumerable<Question>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<Question> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1")]
        [HttpPost]
        public async Task<int> Post(Question question)
        {
            return await _service.Create(question);
        }

        //[Authorize(Roles = "1")]
        [HttpPut("{id}")]
        public async Task<int> Put(Question question)
        {
            return await _service.Update(question);
        }

        //[Authorize(Roles = "1")]
        [HttpDelete("{id}")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
