using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _fromAddress;
        private readonly string _password;
        private readonly string _host;
        private readonly int _port;
        private readonly bool _enableSsl;

        public EmailService(IConfiguration config)
        {
            _fromAddress = config["EmailSettings:FromAddress"] ?? "";
            _password = config["EmailSettings:Password"] ?? "";
            _host = config["EmailSettings:Host"] ?? "smtp.gmail.com";
            _port = int.TryParse(config["EmailSettings:Port"], out var p) ? p : 587;
            _enableSsl = !bool.TryParse(config["EmailSettings:EnableSsl"], out var s) || s;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(_fromAddress) || string.IsNullOrWhiteSpace(_password))
            {
                Console.WriteLine($"[Email] Skipped — SMTP not configured. To: {to} | Subject: {subject}");
                return;
            }

            using var client = new SmtpClient(_host, _port)
            {
                Credentials = new NetworkCredential(_fromAddress, _password),
                EnableSsl = _enableSsl
            };

            using var message = new MailMessage(_fromAddress, to, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(message);
        }

        public async Task SendWithAttachmentAsync(
            string to,
            string subject,
            string body,
            string attachmentFileName,
            Stream attachmentStream)
        {
            if (string.IsNullOrWhiteSpace(_fromAddress) || string.IsNullOrWhiteSpace(_password))
            {
                Console.WriteLine($"[Email] Skipped — SMTP not configured. To: {to} | Subject: {subject} | Attachment: {attachmentFileName}");
                return;
            }

            using var client = new SmtpClient(_host, _port)
            {
                Credentials = new NetworkCredential(_fromAddress, _password),
                EnableSsl = _enableSsl
            };

            using var message = new MailMessage(_fromAddress, to, subject, body)
            {
                IsBodyHtml = true
            };
            message.Attachments.Add(new Attachment(attachmentStream, attachmentFileName));
            await client.SendMailAsync(message);
        }
    }
}
