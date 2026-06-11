using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _apiKey;
        private readonly string _fromAddress;
        private readonly HttpClient _httpClient;

        public EmailService(IConfiguration config, HttpClient httpClient)
        {
            _apiKey = config["Resend:ApiKey"] ?? "";
            _fromAddress = config["Resend:FromAddress"] ?? "";
            _httpClient = httpClient;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            var payload = new
            {
                from = _fromAddress,
                to = new[] { to },
                subject,
                html = body
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
            {
                Headers = { { "Authorization", $"Bearer {_apiKey}" } },
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public async Task SendWithAttachmentAsync(
            string to,
            string subject,
            string body,
            string attachmentFileName,
            Stream attachmentStream)
        {
            using var ms = new MemoryStream();
            await attachmentStream.CopyToAsync(ms);
            var base64Content = Convert.ToBase64String(ms.ToArray());

            var payload = new
            {
                from = _fromAddress,
                to = new[] { to },
                subject,
                html = body,
                attachments = new[]
                {
                    new
                    {
                        filename = attachmentFileName,
                        content = base64Content
                    }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
            {
                Headers = { { "Authorization", $"Bearer {_apiKey}" } },
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }
}
