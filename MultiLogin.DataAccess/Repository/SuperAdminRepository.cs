using MultiLogin.Models;
using MultiUserLoginTrial.DataAccess.Data;
using MultiUserLoginTrial.Repository.IRepository;

namespace MultiUserLoginTrial.Repository
{
    public class SuperAdminRepository : Repository<SuperAdmin>, ISuperAdminRepository
	{
		private ApplicationDBContext _db;
		public SuperAdminRepository(ApplicationDBContext db) : base(db)
		{
			_db = db;
		}

		public void Save()
		{
			_db.SaveChanges();
		}

		public SuperAdmin GetSuperAdminByEmail(string email)
		{
			return _db.SuperAdmin.FirstOrDefault(s => s.SuperAdminEmail == email);
		}


		public void Update(SuperAdmin obj)
		{
			_db.SuperAdmin.Update(obj);
		}
	}
}
