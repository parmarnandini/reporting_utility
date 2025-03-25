namespace ReportingUtility.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public int UserID { get; set; }
        public string Token { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
