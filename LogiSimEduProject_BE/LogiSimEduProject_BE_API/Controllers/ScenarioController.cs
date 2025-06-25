using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScenarioController : ControllerBase
    {
        private readonly IScenarioService _service;

        public ScenarioController(IScenarioService service) => _service = service;
        // GET: api/<ScenarioController>
        [HttpGet]
        public async Task<IEnumerable<Scenario>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<Scenario> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1")]
        [HttpPost]
        public async Task<int> Post(Scenario scenario)
        {
            return await _service.Create(scenario);
        }

        //[Authorize(Roles = "1")]
        [HttpPut("{id}")]
        public async Task<int> Put(Scenario scenario)
        {
            return await _service.Update(scenario);
        }

        //[Authorize(Roles = "1")]
        [HttpDelete("{id}")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
