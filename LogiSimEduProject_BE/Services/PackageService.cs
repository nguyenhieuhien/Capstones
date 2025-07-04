using Repositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IPackageService
    {
        Task<List<Package>> GetAll();
        Task<Package> GetById(string id);
        Task<int> Create(Package package);
        Task<int> Update(Package package);
        Task<bool> Delete(string id);
    }

    public class PackageService : IPackageService
    {
        private PackageRepository _repository;

        public PackageService()
        {
            _repository = new PackageRepository();
        }
        public async Task<int> Create(Package package)
        {
            package.Id = Guid.NewGuid();
            return await _repository.CreateAsync(package);
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

        public async Task<List<Package>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<Package> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> Update(Package package)
        {
            return await _repository.UpdateAsync(package);
        }

    }
}
