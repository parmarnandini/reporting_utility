using ReportingUtility.Models;

namespace ReportingUtility.Repos
{
    public interface IRoleRepository
    {
        IEnumerable<Role> GetAllRoles();
    }
}
