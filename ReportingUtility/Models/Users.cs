using System.Data;

namespace ReportingUtility.Models
{
    public class Users
    {
        public int UserID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int? CompanyID { get; set; }
        public string CompanyName { get; set; } // Extra field for UI display
        public int? RoleID { get; set; }
        public string RoleName { get; set; } // Extra field for UI display
        public string? Loc { get; set; }
        public bool IsActive { get; set; } = true;
        public int? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? PasswordHash { get; set; } // Store hashed password
        public bool IsTempPassword { get; set; } // Track temporary passwords
    }
}
