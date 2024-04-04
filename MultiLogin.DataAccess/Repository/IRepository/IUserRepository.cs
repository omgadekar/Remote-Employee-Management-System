using MultiLogin.Models;
using System.Linq.Expressions;

namespace MultiUserLoginTrial.Repository.IRepository
{
    public interface IUserRepository : IRepository<Users>
    {
        IEnumerable<Users> GetUsersByAdminId(int adminId);

		Users GetUserByEmail(string email);
		Users GetById(int id);
        void Update(Users obj);
        void Save();
    }
}