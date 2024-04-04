using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MultiLogin.Models;
using MultiUserLoginTrial.DataAccess.Data;
using MultiUserLoginTrial.Repository.IRepository;
using System.Linq.Expressions;

namespace MultiUserLoginTrial.Repository
{
    public class UserRepository : Repository<Users>, IUserRepository
    {
        private readonly ApplicationDBContext _db;
		
		public UserRepository(ApplicationDBContext db) : base(db)
        {
            _db = db;
			
		}

        public Users GetById(int id)
        {
            return _db.Users.Find(id);
        }

		public Users GetUserByEmail(string email)
		{
			return _db.Users.FirstOrDefault(u => u.UserEmail == email);
		}
		public IEnumerable<Users> GetUsersByAdminId(int adminId)
        {
            return _db.Users.Where(u => u.AId == adminId).ToList();
        }
        public void Save()
        {
            _db.SaveChanges();
        }
		public void Update(Users obj)
        {
            _db.Users.Update(obj);
        }
    }
}
