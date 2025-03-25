using Dapper;
using Microsoft.Data.SqlClient;
using ReportingUtility.Data;
using ReportingUtility.Models;
using ReportingUtility.Models.DTOs;

namespace ReportingUtility.Repos
{
    public class ReportRepository(DatabaseContext dbContext) : IReportRepository
    {
        private readonly DatabaseContext _dbContext = dbContext;

        public async Task<int> AddReportAsync(Report report, List<string> roleNames)
        {
            using var connection = _dbContext.CreateConnection();

            string getCompanyIdQuery = "SELECT CompanyID FROM Companies WHERE CompanyName = @CompanyName";
            int? companyId = await connection.ExecuteScalarAsync<int?>(getCompanyIdQuery, new { CompanyName = report.CompanyName });

            if (companyId == null)
            {
                throw new Exception("Invalid company name.");
            }
            report.CompanyID = companyId.Value;

            string insertReportQuery = @"
INSERT INTO Reports (ReportName, Category, CompanyID, URL, IsActive, CreatedBy, CreatedOn) 
VALUES (@ReportName, @Category, @CompanyID, @URL, @IsActive, @CreatedBy, GETDATE());
SELECT CAST(SCOPE_IDENTITY() as int);";


            int reportId = await connection.ExecuteScalarAsync<int>(insertReportQuery, new
            {
                ReportName = report.ReportName,   
                Category = report.Category,      
                CompanyID = report.CompanyID,
                URL = report.URL,
                IsActive = true,                  
                CreatedBy = report.CreatedBy
            });


            // Fetch Role IDs
            string getRoleIdsQuery = "SELECT RoleID FROM Roles WHERE RoleName IN @RoleNames";
            var roleIds = await connection.QueryAsync<int>(getRoleIdsQuery, new { RoleNames = roleNames });

            // Insert Report-Role Mappings
            foreach (var roleId in roleIds)
            {
                string insertReportRoleQuery = "INSERT INTO ReportRoles (ReportID, RoleID) VALUES (@ReportID, @RoleID);";
                await connection.ExecuteAsync(insertReportRoleQuery, new { ReportID = reportId, RoleID = roleId });
            }

            return reportId;
        }

        public async Task<bool> UpdateReportAsync(int reportId, Report report, List<string> roleNames)
        {
            using var connection = _dbContext.CreateConnection();

            // Check if report exists
            string checkReportQuery = "SELECT COUNT(*) FROM Reports WHERE ReportID = @ReportID";
            int count = await connection.ExecuteScalarAsync<int>(checkReportQuery, new { ReportID = reportId });
            if (count == 0) return false;

            // Get CompanyID based on CompanyName
            string getCompanyIdQuery = "SELECT CompanyID FROM Companies WHERE CompanyName = @CompanyName";
            int? companyId = await connection.ExecuteScalarAsync<int?>(getCompanyIdQuery, new { CompanyName = report.CompanyName });

            if (companyId == null)
            {
                throw new Exception("Invalid company name.");
            }
            report.CompanyID = companyId.Value;

            // Update report details
            string updateReportQuery = @"
UPDATE Reports 
SET ReportName = @ReportName, Category = @Category, CompanyID = @CompanyID, URL = @URL, 
    IsActive = @IsActive, CreatedBy = @CreatedBy, CreatedOn = GETDATE()
WHERE ReportID = @ReportID;";

            await connection.ExecuteAsync(updateReportQuery, new
            {
                ReportID = reportId,
                ReportName = report.ReportName,
                Category = report.Category,
                CompanyID = report.CompanyID,
                URL = report.URL,
                IsActive = report.IsActive,
                CreatedBy = report.CreatedBy
            });

            // Remove old role mappings
            string deleteRolesQuery = "DELETE FROM ReportRoles WHERE ReportID = @ReportID";
            await connection.ExecuteAsync(deleteRolesQuery, new { ReportID = reportId });

            // Fetch new Role IDs
            string getRoleIdsQuery = "SELECT RoleID FROM Roles WHERE RoleName IN @RoleNames";
            var roleIds = await connection.QueryAsync<int>(getRoleIdsQuery, new { RoleNames = roleNames });

            // Insert new Role Mappings
            foreach (var roleId in roleIds)
            {
                string insertReportRoleQuery = "INSERT INTO ReportRoles (ReportID, RoleID) VALUES (@ReportID, @RoleID);";
                await connection.ExecuteAsync(insertReportRoleQuery, new { ReportID = reportId, RoleID = roleId });
            }

            return true;
        }

        public async Task<bool> DeleteReportAsync(int reportId)
{
    using var connection = _dbContext.CreateConnection();

    // Check if report exists
    string checkReportQuery = "SELECT COUNT(*) FROM Reports WHERE ReportID = @ReportID";
    int count = await connection.ExecuteScalarAsync<int>(checkReportQuery, new { ReportID = reportId });
    if (count == 0) return false;

    // Delete associated role mappings
    string deleteRolesQuery = "DELETE FROM ReportRoles WHERE ReportID = @ReportID";
    await connection.ExecuteAsync(deleteRolesQuery, new { ReportID = reportId });

    // Delete the report
    string deleteReportQuery = "DELETE FROM Reports WHERE ReportID = @ReportID";
    await connection.ExecuteAsync(deleteReportQuery, new { ReportID = reportId });

    return true;
}


        public async Task<IEnumerable<ReportDto>> GetAllReportsAsync()
        {
            using var connection = _dbContext.CreateConnection();

            string query = @"
    SELECT r.ReportID, r.Category, r.ReportName, r.URL, r.CreatedBy, r.CreatedOn, r.IsActive, 
       c.CompanyName, 
       STRING_AGG(ro.RoleName, ', ') AS RoleNames
FROM Reports r
LEFT JOIN Companies c ON r.CompanyID = c.CompanyID
LEFT JOIN ReportRoles rr ON r.ReportID = rr.ReportID
LEFT JOIN Roles ro ON rr.RoleID = ro.RoleID
GROUP BY r.ReportID, r.Category, r.ReportName, r.URL, r.CreatedBy, r.CreatedOn, r.IsActive, c.CompanyName;";

            var result = await connection.QueryAsync<dynamic>(query);  // Explicitly return dynamic

            var reports = result.Select(row => new ReportDto
            {
                 ReportID = (int)row.ReportID,
                ReportName = (string)row.ReportName, 
                Category = row.Category != null ? (string)row.Category : "N/A",
                CompanyName = row.CompanyName != null ? (string)row.CompanyName : "N/A",
                CreatedBy = row.CreatedBy != null ? (int?)row.CreatedBy : null,
                URL = (string)row.URL,
                IsActive = (bool)row.IsActive,
                Roles = row.RoleNames != null
                    ? ((string)row.RoleNames).Split(',').Select(r => r.Trim()).ToList()
                    : new List<string>()
            }).ToList();

            return reports;
        }



        public async Task<IEnumerable<Report>> GetCompanyReportsAsync(int companyId)
        {
            using var connection = _dbContext.CreateConnection();

            string query = @"
    SELECT r.ReportID, r.ReportName, r.URL, r.CreatedBy, r.CreatedOn, r.IsActive, 
           c.CompanyName, 
           STRING_AGG(ro.RoleName, ', ') AS RoleNames
    FROM Reports r
    INNER JOIN Companies c ON r.CompanyID = c.CompanyID
    LEFT JOIN ReportRoles rr ON r.ReportID = rr.ReportID
    LEFT JOIN Roles ro ON rr.RoleID = ro.RoleID
    WHERE r.CompanyID = @CompanyID
    GROUP BY r.ReportID, r.ReportName, r.URL, r.CreatedBy, r.CreatedOn, r.IsActive, c.CompanyName";

            var result = await connection.QueryAsync<Report>(query, new { CompanyID = companyId });

            return result;
        }




        public async Task<IEnumerable<Report>> GetUserReportsAsync(string companyName, string roleName)
        {
            using var connection = _dbContext.CreateConnection();

            string getCompanyIdQuery = "SELECT CompanyID FROM Companies WHERE CompanyName = @CompanyName";
            int? companyId = await connection.ExecuteScalarAsync<int?>(getCompanyIdQuery, new { CompanyName = companyName });

            string getRoleIdQuery = "SELECT RoleID FROM Roles WHERE RoleName = @RoleName";
            int? roleId = await connection.ExecuteScalarAsync<int?>(getRoleIdQuery, new { RoleName = roleName });

            if (companyId == null || roleId == null)
            {
                throw new Exception("Invalid company name or role name.");
            }

            string query = @"
        SELECT r.ReportID, r.ReportName, r.CompanyID, r.URL, r.CreatedBy, r.CreatedOn, r.ModifiedBy, r.ModifiedOn, r.IsActive 
        FROM Reports r
        INNER JOIN ReportRoles rr ON r.ReportID = rr.ReportID
        WHERE r.CompanyID = @CompanyID AND rr.RoleID = @RoleID";

            return await connection.QueryAsync<Report>(query, new { CompanyID = companyId, RoleID = roleId });
        }

    }
}
