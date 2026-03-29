using System.Net;
using System.Net.Mail;
using SmartJobSystem.Server.Data;

namespace SmartJobSystem.Server.Helpers
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly DbHelper _db;

        public EmailService(IConfiguration config, DbHelper db)
        {
            _config = config;
            _db = db;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var encryptionKey = await _db.GetParameterValueAsync("SecuritySettings:EncryptionKey");

            var smtpHostEnc = await _db.GetParameterValueAsync("SmtpSettings:Host");
            var smtpPortEnc = await _db.GetParameterValueAsync("SmtpSettings:Port");
            var smtpUserEnc = await _db.GetParameterValueAsync("SmtpSettings:Username");
            var smtpPassEnc = await _db.GetParameterValueAsync("SmtpSettings:Password");
            var fromEmailEnc = await _db.GetParameterValueAsync("SmtpSettings:FromEmail");

            var smtpHost = SecurityHelper.Decrypt(smtpHostEnc, encryptionKey);
            var smtpPortStr = SecurityHelper.Decrypt(smtpPortEnc, encryptionKey);
            var smtpPort = int.Parse(string.IsNullOrEmpty(smtpPortStr) ? "587" : smtpPortStr);
            var smtpUser = SecurityHelper.Decrypt(smtpUserEnc, encryptionKey);
            var smtpPass = SecurityHelper.Decrypt(smtpPassEnc, encryptionKey);
            var fromEmail = SecurityHelper.Decrypt(fromEmailEnc, encryptionKey);

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
