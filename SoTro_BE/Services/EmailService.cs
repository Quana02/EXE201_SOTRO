using System.Net;
using System.Net.Mail;

namespace SoTro_BE.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpSection = _configuration.GetSection("EmailSettings");
            var host = smtpSection["SmtpServer"];
            var username = smtpSection["SenderEmail"];
            var password = smtpSection["SenderPassword"];
            var fromEmail = smtpSection["SenderEmail"];
            var senderName = smtpSection["SenderName"];

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(fromEmail))
            {
                throw new InvalidOperationException("Cấu hình EmailSettings bị thiếu SmtpServer hoặc SenderEmail.");
            }

            var port = smtpSection.GetValue("SmtpPort", 587);
            var enableSsl = smtpSection.GetValue("EnableSsl", true);

            using var message = new MailMessage
            {
                From = string.IsNullOrWhiteSpace(senderName)
                    ? new MailAddress(fromEmail)
                    : new MailAddress(fromEmail, senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl
            };

            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                client.Credentials = new NetworkCredential(username, password);
            }

            await client.SendMailAsync(message);
        }
    }
}
