using MultiLogin.Models;

namespace MultiUserLoginTrial.Repository.IRepository
{
    public interface ISuperAdminRepository : IRepository<SuperAdmin>
	{
		SuperAdmin GetSuperAdminByEmail(string email);
		void Update(SuperAdmin obj);
		void Save();
	}
}