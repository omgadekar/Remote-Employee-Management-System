using MultiLogin.Models;
using System.Linq.Expressions;

namespace MultiUserLoginTrial.Repository.IRepository
{
    public interface IAdminRepository : IRepository<Admin>
    {
        Task<Admin> GetByIdAsync(int id);
		Admin GetAdminByEmail(string email);
		Admin GetById(int id);
        void Update(Admin obj);
        void Save();
    }
}