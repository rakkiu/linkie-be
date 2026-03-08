
namespace Application.Interfaces 
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <param name="to">To.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <returns></returns>
        Task SendAsync(string to, string subject, string body);
        Task SendWithAttachmentAsync(
            string to,
            string subject,
            string body,
            string attachmentFileName,
            Stream attachmentStream
        );
    }
}
