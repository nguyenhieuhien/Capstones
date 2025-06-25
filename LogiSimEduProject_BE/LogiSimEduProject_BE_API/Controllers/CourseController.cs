using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _service;

        public CourseController(ICourseService service) => _service = service;

        [HttpGet]
        public async Task<IEnumerable<Course>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<Course> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1, 2")]
        [HttpGet("Search")]
        public async Task<IEnumerable<Course>> Get(string name, string description)
        {
            return await _service.Search(name, description);
        }

        //[Authorize(Roles = "1")]
        [HttpPost]
        public async Task<int> Post(Course course)
        {
            return await _service.Create(course);
        }

        //[Authorize(Roles = "1")]
        [HttpPut("{id}")]
        public async Task<int> Put(Course course)
        {
            return await _service.Update(course);
        }

        //[Authorize(Roles = "1")]
        [HttpDelete("{id}")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
