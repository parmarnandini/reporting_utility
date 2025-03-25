using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;

namespace ReportingUtility.Services
{
    public class EmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPass;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(IConfiguration configuration)
        {
            _smtpServer = configuration["EmailSettings:SMTPServer"];
            _smtpPort = int.Parse(configuration["EmailSettings:SMTPPort"]);
            _smtpUser = configuration["EmailSettings:SenderEmail"]; 
            _smtpPass = configuration["EmailSettings:SMTPPassword"];
            _fromEmail = configuration["EmailSettings:SenderEmail"];
            _fromName = configuration["EmailSettings:SenderName"];
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_fromName, _fromEmail));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body // Supports HTML content
            };

            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_smtpUser, _smtpPass);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw;
            }
        }
    }
}
