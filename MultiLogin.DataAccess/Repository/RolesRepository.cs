using MultiLogin.Models;
using MultiUserLoginTrial.DataAccess.Data;
using MultiUserLoginTrial.Repository.IRepository;
using System.Data;

namespace MultiUserLoginTrial.Repository
{
    public class RolesRepository : Repository<Roles>, IRoleRepository
    {
        private ApplicationDBContext _db;
        public RolesRepository(ApplicationDBContext db) : base(db)
        {
            _db = db;
        }

        public Roles FirstOrDefault(Func<Roles, bool> predicate)
        {
            return _db.Roles.FirstOrDefault(predicate);
        }

        public Roles GetById(int id)
        {
            return _db.Roles.Find(id);
        }

        public Roles GetRoleByEmail(string email)
        {
            return _db.Roles.FirstOrDefault(r => r.Emails == email);
        }

        public void Addr(Roles role)
        {
            _db.Roles.Add(role);
        }

        public void Remover(Roles role)
        {
            _db.Roles.Remove(role);
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(Roles obj)
        {
            _db.Roles.Update(obj);
        }

        public IEnumerable<Roles> GetRolesByUserEmails(IEnumerable<string> userEmails)
        {
            return _db.Roles.Where(r => userEmails.Contains(r.Emails)).ToList();
        }
    }
}
