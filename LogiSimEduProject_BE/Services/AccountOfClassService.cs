using Repositories;
using Repositories.Base;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IAccountOfClassService
    {
        Task<List<AccountOfClass>> GetAll();
        Task<AccountOfClass> GetById(string id);
        Task<int> Create(AccountOfClass acCl);
        Task<int> Update(AccountOfClass acCl);
        Task<int> AddStudentToClass(Guid classId, Guid studentId);
        Task<bool> RemoveStudentFromClass(Guid classId, Guid studentId);
    }

    public class AccountOfClassService : IAccountOfClassService
    {
        private AccountOfClassRepository _repository;
        private readonly EnrollmentRequestRepository _enrollmentRepo;
        private readonly ClassRepository _classRepo;

        public AccountOfClassService()
        {
            _repository = new AccountOfClassRepository();
            _enrollmentRepo = new EnrollmentRequestRepository();
            _classRepo = new ClassRepository();
        }
        public async Task<int> Create(AccountOfClass acCl)
        {
            acCl.Id = Guid.NewGuid();
            return await _repository.CreateAsync(acCl);
        }

        public async Task<int> AddStudentToClass(Guid classId, Guid studentId)
        {
            // 1. Kiểm tra học sinh đã được accept vào course chưa
            var request = await _enrollmentRepo.GetAcceptedRequest(studentId);
            if (request == null)
                throw new Exception("Sinh viên chưa được chấp nhận vào khoá học.");

            // 2. Kiểm tra đã trong lớp chưa
            if (await _repository.IsStudentInClass(classId, studentId))
                throw new Exception("Sinh viên đã thuộc lớp này.");

            // 3. Thêm vào class
            var model = new AccountOfClass
            {
                Id = Guid.NewGuid(),
                AccountId = studentId,
                ClassId = classId
            };

            var cls = await _classRepo.GetByIdAsync(classId.ToString());
            if (cls == null) throw new Exception("Không tìm thấy lớp.");
            cls.NumberOfStudent = (cls.NumberOfStudent ?? 0) + 1;

            await _classRepo.UpdateAsync(cls);

            return await _repository.CreateAsync(model);
        }

        public async Task<bool> RemoveStudentFromClass(Guid classId, Guid studentId)
        {
            var record = await _repository.GetStudentInClass(classId, studentId);
            if (record == null)
                throw new Exception("Học sinh không thuộc lớp này.");

            var cls = await _classRepo.GetByIdAsync(classId.ToString());
            if (cls == null) throw new Exception("Không tìm thấy lớp.");

            cls.NumberOfStudent = Math.Max((cls.NumberOfStudent ?? 1) - 1, 0);
            await _classRepo.UpdateAsync(cls);

            return await _repository.RemoveAsync(record);
        }

        public async Task<List<AccountOfClass>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<AccountOfClass> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> Update(AccountOfClass acCl)
        {
            return await _repository.UpdateAsync(acCl);
        }
    }
}
