using Dapper;
using ReportingUtility.Data;
using ReportingUtility.Models;
using ReportingUtility.Services;
using System.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace ReportingUtility.Repos
{
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseContext _db;
        private readonly EmailService _emailService;

        public UserRepository(DatabaseContext db, EmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        public async Task<IEnumerable<Users>> GetAllUsersAsync()
        {
            string query = @"
            SELECT U.*, C.CompanyName, R.RoleName 
            FROM Users U
            LEFT JOIN Companies C ON U.CompanyID = C.CompanyID
            LEFT JOIN Roles R ON U.RoleID = R.RoleID
            WHERE U.isActive = 1
            AND R.RoleName != 'Master Admin' ";

            using (var connection = _db.CreateConnection())
            {
                return await connection.QueryAsync<Users>(query);
            }
        }

        public async Task<Users> GetUserByEmailAsync(string email)
        {
            string query = @"
    SELECT U.*, C.CompanyName, R.RoleName 
    FROM Users U
    LEFT JOIN Companies C ON U.CompanyID = C.CompanyID
    LEFT JOIN Roles R ON U.RoleID = R.RoleID
    WHERE U.Email = @Email";

            using (var connection = _db.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Users>(query, new { Email = email });
            }
        }

        public async Task<Users> GetUserByIdAsync(int userId)
        {
            string query = @"
            SELECT U.*, C.CompanyName, R.RoleName 
            FROM Users U
            LEFT JOIN Companies C ON U.CompanyID = C.CompanyID
            LEFT JOIN Roles R ON U.RoleID = R.RoleID
            WHERE U.UserID = @UserID";

            using (var connection = _db.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Users>(query, new { UserID = userId });
            }
        }

        public async Task<IEnumerable<Users>> GetUsersByCompanyAsync(int companyId)
        {
            string query = @"
    SELECT U.*, C.CompanyName, R.RoleName 
    FROM Users U
    LEFT JOIN Companies C ON U.CompanyID = C.CompanyID
    LEFT JOIN Roles R ON U.RoleID = R.RoleID
    WHERE U.CompanyID = @CompanyID AND U.isActive = 1 AND R.RoleName != 'Company Admin'";

            using (var connection = _db.CreateConnection())
            {
                return await connection.QueryAsync<Users>(query, new { CompanyID = companyId });
            }
        }



        //public async Task<int> AddUserAsync(Users user)
        //{
        //    string query = @"
        //    INSERT INTO Users (Name, Email, CompanyID, RoleID, Loc, isActive, CreatedBy, CreatedOn) 
        //    VALUES (@Name, @Email, @CompanyID, @RoleID, @Loc, 1, @CreatedBy, GETDATE());
        //    SELECT CAST(SCOPE_IDENTITY() as int);";

        //    using (var connection = _db.CreateConnection()) 
        //    {
        //        return await connection.ExecuteScalarAsync<int>(query, user);
        //    }
        //}

        private string GenerateTempPassword()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        //public async Task<int> AddUserAsync(Users user)
        //{
        //    string tempPassword = GenerateTempPassword();
        //    user.PasswordHash = HashPassword(tempPassword);
        //    user.IsTempPassword = true;

        //    string query = @"
        //    INSERT INTO Users (Name, Email, CompanyID, RoleID, Loc, isActive, CreatedBy, CreatedOn, PasswordHash, IsTempPassword) 
        //    VALUES (@Name, @Email, @CompanyID, @RoleID, @Loc, 1, @CreatedBy, GETDATE(), @PasswordHash, @IsTempPassword);
        //    SELECT CAST(SCOPE_IDENTITY() as int);";

        //    using (var connection = _db.CreateConnection())
        //    {
        //        int userId = await connection.ExecuteScalarAsync<int>(query, user);

        //        // Send email with temporary password
        //        string emailBody = $@"
        //            <p>Hello {user.Name},</p>
        //            <p>Your account has been created. Your temporary password is:</p>
        //            <p><strong>{tempPassword}</strong></p>
        //            <p>Please log in and change your password immediately.</p>
        //        <p>Best Regards,<br>Ethics Reporting Utility Team</p>";

        //        await _emailService.SendEmailAsync(user.Email, "Temporary Password", emailBody);

        //        return userId;
        //    }
        //}


        public async Task<int> AddUserAsync(Users user)
        {
            string getCompanyIdQuery = "SELECT CompanyID FROM Companies WHERE CompanyName = @CompanyName";
            string getRoleIdQuery = "SELECT RoleID FROM Roles WHERE RoleName = @RoleName";

            using (var connection = _db.CreateConnection())
            {
                // Retrieve CompanyID and RoleID using CompanyName and RoleName from user object
                int? companyId = await connection.ExecuteScalarAsync<int?>(getCompanyIdQuery, new { CompanyName = user.CompanyName });
                int? roleId = await connection.ExecuteScalarAsync<int?>(getRoleIdQuery, new { RoleName = user.RoleName });

                if (companyId == null || roleId == null)
                {
                    throw new Exception("Invalid company name or role name.");
                }

                // Assign retrieved IDs back to user object
                user.CompanyID = companyId.Value;
                user.RoleID = roleId.Value;

                string tempPassword = GenerateTempPassword();
                user.PasswordHash = HashPassword(tempPassword);
                user.IsTempPassword = true;

                string query = @"
        INSERT INTO Users (Name, Email, CompanyID, RoleID, Loc, isActive, CreatedBy, CreatedOn, PasswordHash, IsTempPassword) 
        VALUES (@Name, @Email, @CompanyID, @RoleID, @Loc, 1, @CreatedBy, GETDATE(), @PasswordHash, @IsTempPassword);
        SELECT CAST(SCOPE_IDENTITY() as int);";

                int userId = await connection.ExecuteScalarAsync<int>(query, user);

                string emailBody = $@"
            <p>Hello {user.Name},</p>
            <p>Your account has been created. Your temporary password is:</p>
            <p><strong>{tempPassword}</strong></p>
            <p>Please log in and change your password immediately.</p>
            <p>Best Regards,<br>Ethics Reporting Utility Team</p>";

                await _emailService.SendEmailAsync(user.Email, "Temporary Password", emailBody);

                return userId;
            }
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string newPassword)
        {
            string hashedPassword = HashPassword(newPassword);
            string query = @"
            UPDATE Users SET PasswordHash = @PasswordHash, IsTempPassword = 0
            WHERE UserID = @UserID AND IsTempPassword = 1;";

            using (var connection = _db.CreateConnection())
            {
                return await connection.ExecuteAsync(query, new { PasswordHash = hashedPassword, UserID = userId }) > 0;
            }
        }

        public bool VerifyPassword(Users user, string password)
        {
            string hashedInputPassword = HashPassword(password);
            return user.PasswordHash == hashedInputPassword;
        }


        public async Task<bool> UpdateUserAsync(Users user)
        {
            string getCompanyIdQuery = "SELECT CompanyID FROM Companies WHERE CompanyName = @CompanyName";
            string getRoleIdQuery = "SELECT RoleID FROM Roles WHERE RoleName = @RoleName";

            using (var connection = _db.CreateConnection())
            {
                int? companyId = await connection.ExecuteScalarAsync<int?>(getCompanyIdQuery, new { CompanyName = user.CompanyName });
                int? roleId = await connection.ExecuteScalarAsync<int?>(getRoleIdQuery, new { RoleName = user.RoleName });

                if (companyId == null || roleId == null)
                {
                    throw new Exception("Invalid company name or role name.");
                }

                user.CompanyID = companyId.Value;
                user.RoleID = roleId.Value;

                string query = @"
        UPDATE Users SET 
            Name = @Name, 
            Email = @Email, 
            CompanyID = @CompanyID, 
            RoleID = @RoleID, 
            Loc = @Loc, 
            ModifiedBy = @ModifiedBy, 
            ModifiedOn = GETDATE()
        WHERE UserID = @UserID";

                return await connection.ExecuteAsync(query, user) > 0;
            }
        }


        public async Task<bool> DeleteUserAsync(int userId)
        {
            string query = @"
    UPDATE Users 
    SET isActive = 0, ModifiedOn = GETDATE() 
    WHERE UserID = @UserID";

            using (var connection = _db.CreateConnection())
            {
                return await connection.ExecuteAsync(query, new { UserID = userId }) > 0;
            }
        }


        public async Task<bool> RequestPasswordResetAsync(string email)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null)
                return false; 

            string token = Guid.NewGuid().ToString(); 
            DateTime expiryDate = DateTime.UtcNow.AddHours(1); // 1-hour expiration

            await CreatePasswordResetTokenAsync(user.UserID, token, expiryDate);

            string resetLink = $"http://localhost:3000/reset-password?token={token}";
            string emailBody = $@"
        <p>Hello {user.Name},</p>
        <p>Click the link below to reset your password:</p>
        <p><a href='{resetLink}'>Reset Password</a></p>
        <p>This link is valid for 1 hour.</p>";

            await _emailService.SendEmailAsync(user.Email, "Password Reset", emailBody);
            return true;
        }

        public async Task<bool> CreatePasswordResetTokenAsync(int userId, string token, DateTime expiryDate)
        {
            string sql = "INSERT INTO PasswordResetTokens (UserID, Token, ExpiryDate) VALUES (@UserID, @Token, @ExpiryDate)";
            using (var connection = _db.CreateConnection())
            {
                int rowsAffected = await connection.ExecuteAsync(sql, new { UserID = userId, Token = token, ExpiryDate = expiryDate });
                return rowsAffected > 0;
            }
            
        }

        public async Task<PasswordResetToken> GetValidResetTokenAsync(string token)
        {
            using (var connection = _db.CreateConnection())
            {
                // Delete expired tokens before fetching
                await connection.ExecuteAsync("DELETE FROM PasswordResetTokens WHERE ExpiryDate <= @CurrentTime", new { CurrentTime = DateTime.UtcNow });

                string sql = "SELECT * FROM PasswordResetTokens WHERE Token = @Token AND ExpiryDate > @CurrentTime";
                return await connection.QueryFirstOrDefaultAsync<PasswordResetToken>(
                    sql, new { Token = token, CurrentTime = DateTime.UtcNow });
            }
        }



        public async Task<bool> UpdateForgotPasswordAsync(int userId, string newPassword)
        {
            string hashedPassword = HashPassword(newPassword);
            string sql = "UPDATE Users SET PasswordHash = @NewPassword WHERE UserID = @UserID";
            using (var connection = _db.CreateConnection())
            {
                int rowsAffected = await connection.ExecuteAsync(sql, new { UserID = userId, NewPassword = hashedPassword });
                return rowsAffected > 0;
            }
        }

      
        public async Task<bool> DeleteResetTokenAsync(string token)
        {
            string sql = "DELETE FROM PasswordResetTokens WHERE Token = @Token";
            using (var connection = _db.CreateConnection())
            {
                int rowsAffected = await connection.ExecuteAsync(sql, new { Token = token });
                return rowsAffected > 0;
            }
        }

    }
}
