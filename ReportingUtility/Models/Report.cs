//namespace ReportingUtility.Models
//{
//    public class Report
//    {
//        public int ReportID { get; set; }
//        public string ReportName { get; set; }
//        public string? Category { get; set; }
//        public int? CompanyID { get; set; }
//        public string URL { get; set; }
//        public int? CreatedBy { get; set; }
//        public DateTime CreatedOn { get; set; }
//        public string? ModifiedBy { get; set; }
//        public DateTime? ModifiedOn { get; set; }



//        public string CompanyName { get; set; } // Extra field for UI display
//        public string Roles{ get; set; } // Extra field for UI display
//        //public Companies Company { get; set; }
//        public List<ReportRole> ReportRoles { get; set; }
//    }
//}

using ReportingUtility.Models;

public class Report
{
    public int ReportID { get; set; }
    public string ReportName { get; set; }
    public string? Category { get; set; }
    public int? CompanyID { get; set; } // Keep for database relations
    public string CompanyName { get; set; } // For UI display
    public int? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string URL { get; set; }
    public bool IsActive { get; set; } // Add to match DTO
    public List<string> Roles { get; set; } 

    // Navigation Property
    public List<ReportRole> ReportRoles { get; set; }
}

