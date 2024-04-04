using MultiLogin.Models;
using MultiUserLoginTrial.DataAccess.Data;
using MultiUserLoginTrial.Repository.IRepository;
using System.Linq.Expressions;

namespace MultiUserLoginTrial.Repository
{
    public class AdminRepository : Repository<Admin>, IAdminRepository
    {
        private ApplicationDBContext _db;
        public AdminRepository(ApplicationDBContext db) : base(db)
        {
            _db = db;
        }

		public async Task<Admin> GetByIdAsync(int id)
        {
            return await _db.Admin.FindAsync(id);
        }

        public Admin GetById(int id)
        {
            return _db.Admin.Find(id);
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(Admin obj)
        {
            _db.Admin.Update(obj);
        }

		public Admin GetAdminByEmail(string email)
		{
			return _db.Admin.FirstOrDefault(u => u.AdminEmail == email);
		}
	}
}
