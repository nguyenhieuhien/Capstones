using Repositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IPackageTypeService
    {
        Task<List<PackageType>> GetAll();
        Task<PackageType> GetById(string id);
        Task<int> Create(PackageType packageType);
        Task<int> Update(PackageType packageType);
        Task<bool> Delete(string id);
    }

    public class PackageTypeService : IPackageTypeService
    {
        private PackageTypeRepository _repository;

        public PackageTypeService()
        {
            _repository = new PackageTypeRepository();
        }
        public async Task<int> Create(PackageType packageType)
        {
            packageType.Id = Guid.NewGuid();
            return await _repository.CreateAsync(packageType);
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

        public async Task<List<PackageType>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<PackageType> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> Update(PackageType packageType)
        {
            return await _repository.UpdateAsync(packageType);
        }

    }
}
