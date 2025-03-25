using ReportingUtility.Models;

namespace ReportingUtility.Repos
{
    public interface IUserRepository
    {
        Task<IEnumerable<Users>> GetAllUsersAsync();
        Task<Users> GetUserByIdAsync(int userId);
        Task<Users> GetUserByEmailAsync(string email);
        Task<IEnumerable<Users>> GetUsersByCompanyAsync(int companyId);
        Task<int> AddUserAsync(Users user);
        Task<bool> UpdateUserAsync(Users user);
        Task<bool> DeleteUserAsync(int userId);
        bool VerifyPassword(Users user, string password);
        Task<bool> UpdatePasswordAsync(int userId, string newPassword);
        Task<bool> RequestPasswordResetAsync(string email);
        Task<bool> CreatePasswordResetTokenAsync(int userId, string token, DateTime expiryDate);
        Task<PasswordResetToken> GetValidResetTokenAsync(string token);
        Task<bool> UpdateForgotPasswordAsync(int userId, string newPassword);
        Task<bool> DeleteResetTokenAsync(string token);
    }
}
