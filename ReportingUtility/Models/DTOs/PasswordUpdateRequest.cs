namespace ReportingUtility.Models.DTOs
{
    public class PasswordUpdateRequest
    {
        public int UserId { get; set; }
        public string NewPassword { get; set; }
    }
}
