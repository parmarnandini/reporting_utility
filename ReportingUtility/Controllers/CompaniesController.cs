using Microsoft.AspNetCore.Mvc;
using ReportingUtility.Repos;
using Microsoft.AspNetCore.Authorization;
using ReportingUtility.Services;

namespace ReportingUtility.Controllers
{
    [ApiController]
    [Route("api/companies")]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly JwtService _jwtService;

        public CompaniesController(ICompanyRepository companyRepository, JwtService jwtService)
        {
            _companyRepository = companyRepository;
            _jwtService = jwtService;
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetCompanies()
        {
            var companies = _companyRepository.GetAllCompanies();
            return Ok(companies);
        }

        [Authorize]
        [HttpPost]
        public IActionResult AddCompany([FromBody] string companyName)
        {
            _companyRepository.AddCompany(companyName);
            return Ok(new { message = "Company added successfully" });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCompany(int id)
        {
            _companyRepository.DeleteCompany(id);
            return Ok(new { message = "Company deleted successfully" });
        }

    }
}
