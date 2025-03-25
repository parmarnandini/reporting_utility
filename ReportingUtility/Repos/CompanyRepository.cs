using Dapper;
using Microsoft.Data.SqlClient;
using ReportingUtility.Data;
using ReportingUtility.Models;

namespace ReportingUtility.Repos
{
    public class CompanyRepository(DatabaseContext dbContext) : ICompanyRepository
    {
        private readonly DatabaseContext _dbContext = dbContext;

        public IEnumerable<Companies> GetAllCompanies()
        {
            using var connection = _dbContext.CreateConnection();
            return connection.Query<Companies>("SELECT CompanyID, CompanyName FROM Companies"); 
        }

        public void AddCompany(string companyName)
        {
            using var connection = _dbContext.CreateConnection();
            string query = "INSERT INTO Companies (CompanyName) VALUES (@CompanyName)";
            connection.Execute(query, new { CompanyName = companyName });
        }


        public void DeleteCompany(int companyId)
        {
            using var connection = _dbContext.CreateConnection();
            connection.Execute("DELETE FROM Companies WHERE CompanyID = @companyId", new { companyId });

        }
    }
}
