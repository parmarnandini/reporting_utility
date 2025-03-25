using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ReportingUtility.Models;
using ReportingUtility.Repos;
using ReportingUtility.Services;
using ReportingUtility.Models.DTOs;

namespace ReportingUtility.Controllers
{
   
        [ApiController]
        [Route("api/reports")]
        public class ReportController : ControllerBase
        {
            private readonly IReportRepository _reportRepository;
            private readonly JwtService _jwtService;


        public ReportController(IReportRepository reportRepository, JwtService jwtService)
            {
                _reportRepository = reportRepository;
                _jwtService = jwtService;
            }

        [Authorize]
        [HttpPost("add-report")]
            public async Task<IActionResult> AddReport([FromBody] ReportDto reportDto)
            {
                try
                {
                    var report = new Report
                    {
                        ReportName = reportDto.ReportName,
                        CompanyName = reportDto.CompanyName,
                        Category = reportDto.Category,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = reportDto.CreatedBy,
                        URL = reportDto.URL,
                        Roles = reportDto.Roles
                    };

                    int reportId = await _reportRepository.AddReportAsync(report, reportDto.Roles);
                    return Ok(new { ReportID = reportId });
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

        [Authorize]
        [HttpPut("update-report/{id}")]
        public async Task<IActionResult> UpdateReport(int id, [FromBody] ReportDto reportDto)
        {
            try
            {
                var report = new Report
                {
                    ReportName = reportDto.ReportName,
                    CompanyName = reportDto.CompanyName,
                    Category = reportDto.Category,
                    CreatedBy = reportDto.CreatedBy,
                    URL = reportDto.URL,
                    IsActive = reportDto.IsActive,
                    Roles = reportDto.Roles
                };

                bool updated = await _reportRepository.UpdateReportAsync(id, report, reportDto.Roles);
                if (!updated) return NotFound("Report not found.");

                return Ok(new { Message = "Report updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpDelete("delete-report/{id}")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            try
            {
                bool deleted = await _reportRepository.DeleteReportAsync(id);
                if (!deleted) return NotFound("Report not found.");

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [Authorize]
        [HttpGet("get-all-reports")]
        public async Task<IActionResult> GetAllReports()
        {
            try
            {
                var reports = await _reportRepository.GetAllReportsAsync();
                return Ok(reports);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("get-company-reports")]
        public async Task<IActionResult> GetCompanyReports([FromQuery] int companyId)
        {
            try
            {
                var reports = await _reportRepository.GetCompanyReportsAsync(companyId);
                return Ok(reports);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("get-reports")]
            public async Task<IActionResult> GetReports([FromQuery] string companyName, [FromQuery] string roleName)
            {
                try
                {
                    var reports = await _reportRepository.GetUserReportsAsync(companyName, roleName);
                    return Ok(reports);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }

    }

