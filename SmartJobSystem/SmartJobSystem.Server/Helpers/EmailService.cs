using System.Net;
using System.Net.Mail;

namespace SmartJobSystem.Server.Helpers
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpHost = _config["SmtpSettings:Host"];
            var smtpPort = int.Parse(_config["SmtpSettings:Port"] ?? "587");
            var smtpUser = _config["SmtpSettings:Username"];
            var smtpPass = _config["SmtpSettings:Password"];
            var fromEmail = _config["SmtpSettings:FromEmail"];

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser))
            {
                // Fallback or log if not configured
                Console.WriteLine($"Email to {toEmail} NOT SENT (SMTP not configured): {subject} - {body}");
                return;
            }

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail ?? smtpUser),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}
