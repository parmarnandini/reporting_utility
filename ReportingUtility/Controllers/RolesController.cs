using Microsoft.AspNetCore.Mvc;
using ReportingUtility.Repos;
using ReportingUtility.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReportingUtility.Controllers
{
    [Route("api/roles")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRoleRepository _roleRepo;

        public RolesController(IRoleRepository roleRepo)
        {
            _roleRepo = roleRepo;
        }

        [HttpGet("getAllRoles")]
        public IActionResult GetAllRoles()
        {
            var roles = _roleRepo.GetAllRoles();
            return Ok(roles);
        }
    }
}
