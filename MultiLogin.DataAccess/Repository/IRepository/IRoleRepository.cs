using MultiLogin.Models;
using System.Data;

namespace MultiUserLoginTrial.Repository.IRepository
{
    public interface IRoleRepository: IRepository<Roles>
    {


        IEnumerable<Roles> GetRolesByUserEmails(IEnumerable<string> userEmails);
        Roles GetById(int id);
        Roles GetRoleByEmail(string email);
        void Addr(Roles role);
        void Remover(Roles role);
        void Update(Roles obj);

        Roles FirstOrDefault(Func<Roles, bool> predicate);
        void Save();
    }
}