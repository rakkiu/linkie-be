using Application.Interfaces;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        public Task SendAsync(string to, string subject, string body)
        {
            // TODO: integrate SMTP / SendGrid
            Console.WriteLine($"[Email] To: {to} | Subject: {subject}");
            return Task.CompletedTask;
        }

        public Task SendWithAttachmentAsync(
            string to,
            string subject,
            string body,
            string attachmentFileName,
            Stream attachmentStream)
        {
            // TODO: integrate SMTP / SendGrid with attachment support
            Console.WriteLine($"[Email] To: {to} | Subject: {subject} | Attachment: {attachmentFileName}");
            return Task.CompletedTask;
        }
    }
}
