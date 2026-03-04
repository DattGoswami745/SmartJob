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
            var encryptionKey = _config["SecuritySettings:EncryptionKey"];

            var smtpHost = SecurityHelper.Decrypt(_config["SmtpSettings:Host"], encryptionKey);
            var smtpPortStr = SecurityHelper.Decrypt(_config["SmtpSettings:Port"], encryptionKey);
            var smtpPort = int.Parse(string.IsNullOrEmpty(smtpPortStr) ? "587" : smtpPortStr);
            var smtpUser = SecurityHelper.Decrypt(_config["SmtpSettings:Username"], encryptionKey);
            var smtpPass = SecurityHelper.Decrypt(_config["SmtpSettings:Password"], encryptionKey);
            var fromEmail = SecurityHelper.Decrypt(_config["SmtpSettings:FromEmail"], encryptionKey);

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

            var htmlView = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "sjslogo.png");
            if (System.IO.File.Exists(logoPath))
            {
                var logo = new LinkedResource(logoPath, "image/png")
                {
                    ContentId = "sjslogocid"
                };
                logo.ContentType.Name = "sjslogo.png";
                htmlView.LinkedResources.Add(logo);
            }

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail ?? smtpUser,"SmartJobSystem"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            
            mailMessage.AlternateViews.Add(htmlView);

            mailMessage.To.Add(toEmail);

            try
            {
                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMAIL ERROR] Failed to send email to {toEmail} - Subject: {subject}");
                Console.WriteLine($"[EMAIL EXCEPTION] {ex.Message}");
                if (ex.InnerException != null)
                {
                     Console.WriteLine($"[EMAIL INNER EXCEPTION] {ex.InnerException.Message}");
                }
                throw; // Rethrow to let the caller handle it if needed
            }
        }
    }
}
