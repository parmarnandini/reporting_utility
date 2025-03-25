using Dapper;
using ReportingUtility.Models;
using Microsoft.Data.SqlClient;
using ReportingUtility.Data;

namespace ReportingUtility.Repos
{
    public class RoleRepository : IRoleRepository
    {
        private readonly DatabaseContext _db;
        public RoleRepository(DatabaseContext db)
        {
            _db = db;
        }

        public IEnumerable<Role> GetAllRoles()
        {
            using var connection = _db.CreateConnection();
            return connection.Query<Role>("SELECT RoleID, RoleName FROM Roles");
        }
    }
}
