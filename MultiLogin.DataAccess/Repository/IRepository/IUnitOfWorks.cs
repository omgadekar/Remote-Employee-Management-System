

namespace MultiUserLoginTrial.Repository.IRepository
{
    public interface IUnitOfWorks
    {
        IAdminRepository Admin { get; }

        IRoleRepository Roles { get; }

        IUserRepository Users { get; }

		ISuperAdminRepository SuperAdmin { get; }

		void Save();
    }
}
