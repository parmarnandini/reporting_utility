namespace ReportingUtility.Models.DTOs
{
    public class ReportDto
    {
        public int ReportID { get; set; }
        public string ReportName { get; set; } 
        public string? Category { get; set; }   
        public string CompanyName { get; set; }
        public int? CreatedBy { get; set; }
        public string URL { get; set; }
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; }
    }
}
