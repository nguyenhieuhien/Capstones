using Repositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface ICourseService
    {
        Task<List<Course>> GetAll();
        Task<Course> GetById(string id);
        Task<int> Create(Course course);
        Task<int> Update(Course course);
        Task<bool> Delete(string id);
        Task<List<Course>> Search(string name, string description);
    }

    public class CourseService : ICourseService
    {
        private CourseRepository _repository;

        public CourseService()
        {
            _repository = new CourseRepository();
        }
        public async Task<int> Create(Course course)
        {
            course.Id = Guid.NewGuid();
            return await _repository.CreateAsync(course);
        }

        public async Task<bool> Delete(string id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item != null)
            {
                return await _repository.RemoveAsync(item);
            }

            return false;
        }

        public async Task<List<Course>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<Course> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<List<Course>> Search(string name, string description)
        {
            return await _repository.Search(name, description);
        }

        public async Task<int> Update(Course course)
        {
            return await _repository.UpdateAsync(course);
        }
    }
}
