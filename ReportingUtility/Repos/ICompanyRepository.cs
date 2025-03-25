using ReportingUtility.Models;
namespace ReportingUtility.Repos
{
    public interface ICompanyRepository
    {
        IEnumerable<Companies> GetAllCompanies();
        void AddCompany(string companyName);
        void DeleteCompany(int id);

    }
}
