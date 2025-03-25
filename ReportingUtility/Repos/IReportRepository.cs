using ReportingUtility.Models;
using ReportingUtility.Models.DTOs;

namespace ReportingUtility.Repos
{
    public interface IReportRepository
    {
        Task<int> AddReportAsync(Report report, List<string> roleNames);
        Task<bool> UpdateReportAsync(int reportId, Report report, List<string> roleNames);
        Task<bool> DeleteReportAsync(int reportId);
        Task<IEnumerable<ReportDto>> GetAllReportsAsync();
        Task<IEnumerable<Report>> GetCompanyReportsAsync(int companyId);
        Task<IEnumerable<Report>> GetUserReportsAsync(string companyName, string roleName);
    }
}
