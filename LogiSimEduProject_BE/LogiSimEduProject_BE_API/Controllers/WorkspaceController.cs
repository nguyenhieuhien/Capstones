using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkspaceController : ControllerBase
    {
        private readonly IWorkspaceService _service;
        public WorkspaceController(IWorkspaceService service) => _service = service;

        // GET: api/<WorkspaceController>
        [HttpGet]
        public async Task<IEnumerable<Workspace>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<Workspace> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1")]
        [HttpPost]
        public async Task<int> Post(Workspace workspace)
        {
            return await _service.Create(workspace);
        }

        //[Authorize(Roles = "1")]
        [HttpPut("{id}")]
        public async Task<int> Put(Workspace workspace)
        {
            return await _service.Update(workspace);
        }

        //[Authorize(Roles = "1")]
        [HttpDelete("{id}")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
