using MultiUserLoginTrial.DataAccess.Data;
using MultiUserLoginTrial.Repository.IRepository;

namespace MultiUserLoginTrial.Repository
{
    public class UnitOfWorks : IUnitOfWorks
    {
        private readonly ApplicationDBContext _db;
        public IAdminRepository Admin { get; set; }

        public IRoleRepository Roles { get; set; }

        public IUserRepository Users { get; set; }

		public ISuperAdminRepository SuperAdmin {  get; set; }

		public UnitOfWorks(ApplicationDBContext _db)
        {
            this._db = _db;
            Admin = new AdminRepository(_db);
            Roles = new RolesRepository(_db);
            Users = new UserRepository(_db);
            SuperAdmin = new SuperAdminRepository(_db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
