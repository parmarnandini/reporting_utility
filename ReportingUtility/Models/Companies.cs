namespace ReportingUtility.Models
{
    public class Companies
    {
        public int CompanyID { get; set; }
        public string? CompanyName { get; set; }
        public bool isActive { get; set; } = true;
        public int? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
