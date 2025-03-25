using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReportingUtility.Models;
using ReportingUtility.Repos;
using ReportingUtility.Services;
using ReportingUtility.Models.DTOs;


namespace ReportingUtility.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly JwtService _jwtService;
        private readonly EmailService _emailService;
        public UserController(IUserRepository userRepo, JwtService jwtService, EmailService emailService)
        {
            _userRepo = userRepo;
            _jwtService = jwtService;
            _emailService = emailService;
        }


        [Authorize]
        [HttpGet("getAllUsers")]
        public async Task<ActionResult<IEnumerable<Users>>> GetAllUsers()
        {
            var users = await _userRepo.GetAllUsersAsync();
            return Ok(users);
        }

        [Authorize]
        [HttpGet("getUserById/{id}")]
        public async Task<ActionResult<Users>> GetUserById(int id)
        {
            var user = await _userRepo.GetUserByIdAsync(id);
            if (user == null) return NotFound("User not found.");
            return Ok(user);
        }


        [Authorize]
        [HttpGet("getUsersByCompany/{companyId}")]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsersByCompany(int companyId)
        {
            var users = await _userRepo.GetUsersByCompanyAsync(companyId);
            if (users == null || !users.Any()) return NotFound("No users found for this company.");
            return Ok(users);
        }

        [Authorize]
        [HttpPost("createUser")]
        public async Task<ActionResult<int>> CreateUser([FromBody] Users user)
        {
            if (user == null) return BadRequest("Invalid user data.");
            var userId = await _userRepo.AddUserAsync(user);
            return CreatedAtAction(nameof(GetUserById), new { id = userId }, user);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userRepo.GetUserByEmailAsync(request.Email);
            if (user == null) return Unauthorized(new { message = "Invalid credentials." });

            if (!_userRepo.VerifyPassword(user, request.Password))
                return Unauthorized(new { message = "Invalid credentials." });

            var token = _jwtService.GenerateToken(user);

            if (user.IsTempPassword)
                return Ok(new
                {
                    message = "Temporary password detected. Please update your password.",
                    forceChange = true,
                    userId = user.UserID
                });

            return Ok(new
            {
                message = "Login successful.",
                token,
                userId = user.UserID,
                role = user.RoleName,
                companyId = user.CompanyID,
                forceChange = false
            });
        }

        [Authorize]
        [HttpGet("protected-resource")]
        public IActionResult ProtectedResource()
        {
            return Ok(new { message = "You are authorized!" });
        }

        [HttpPost("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] PasswordUpdateRequest request)
        {
            var success = await _userRepo.UpdatePasswordAsync(request.UserId, request.NewPassword);
            if (!success) return BadRequest("Password update failed.");
            return Ok("Password updated successfully.");
        }

        [Authorize]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] Users user)
        {
            if (user == null || id != user.UserID) return BadRequest("Invalid user data.");
            var updated = await _userRepo.UpdateUserAsync(user);
            if (!updated) return NotFound("User not found.");
            return NoContent();
        }

        [Authorize]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var deleted = await _userRepo.DeleteUserAsync(id);
            if (!deleted) return NotFound("User not found.");
            return NoContent();
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            bool success = await _userRepo.RequestPasswordResetAsync(request.Email);
            if (!success)
                return NotFound("User does not exist.");

            return Ok("Reset link sent to your email.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var resetToken = await _userRepo.GetValidResetTokenAsync(request.Token);
            if (resetToken == null)
                return BadRequest("Invalid or expired token.");

            // Update password
            bool success = await _userRepo.UpdateForgotPasswordAsync(resetToken.UserID, request.NewPassword);
            if (!success) return BadRequest("Failed to reset password.");

            // Delete token after use
            await _userRepo.DeleteResetTokenAsync(request.Token);

            return Ok("Password has been reset successfully.");
        }


    }
}

