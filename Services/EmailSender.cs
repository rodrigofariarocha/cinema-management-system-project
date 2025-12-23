using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace CinemaRocha.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;
        private readonly IConfiguration _configuration;

        public EmailSender(ILogger<EmailSender> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpEnabled = _configuration.GetValue<bool>("EmailSettings:SmtpEnabled");

            if (smtpEnabled)
            {
                await SendViaSmtpAsync(email, subject, htmlMessage);
            }
            else
            {
                await SaveToFileAsync(email, subject, htmlMessage);
            }
        }

        private async Task SendViaSmtpAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"];
                var smtpPort = _configuration.GetValue<int>("EmailSettings:SmtpPort");
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"];

                using var smtpClient = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully via SMTP to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email via SMTP to {Email}", email);
                // Fallback to file
                await SaveToFileAsync(email, subject, htmlMessage);
            }
        }

        private async Task SaveToFileAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var emailsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "emails");
                Directory.CreateDirectory(emailsPath);

                var fileName = $"Email_{DateTime.Now:yyyyMMdd_HHmmss}_{email.Replace("@", "_at_")}.html";
                var filePath = Path.Combine(emailsPath, fileName);

                var emailContent = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>{subject}</title>
</head>
<body>
    <div style='padding: 20px; background-color: #f5f5f5;'>
        <div style='max-width: 600px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px;'>
            <h3>Para: {email}</h3>
            <h3>Assunto: {subject}</h3>
            <hr>
            {htmlMessage}
        </div>
    </div>
</body>
</html>";

                await File.WriteAllTextAsync(filePath, emailContent);
                
                _logger.LogInformation("Email saved to file: {FilePath}", filePath);
                _logger.LogInformation("EMAIL (File): To={Email}, Subject={Subject}", email, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save email to file for {Email}", email);
            }
        }
    }
}
