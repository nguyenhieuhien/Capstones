using Repositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IClassService
    {
        Task<List<Class>> GetAll();
        Task<Class> GetById(string id);
        Task<int> Create(Class _class);
        Task<int> Update(Class _class);
        Task<bool> Delete(string id);
    }

    public class ClassService : IClassService
    {
        private ClassRepository _repository;

        public ClassService()
        {
            _repository = new ClassRepository();
        }
        public async Task<int> Create(Class _class)
        {
            _class.Id = Guid.NewGuid();
            return await _repository.CreateAsync(_class);
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

        public async Task<List<Class>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<Class> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> Update(Class _class)
        {
            return await _repository.UpdateAsync(_class);
        }
    }
}
