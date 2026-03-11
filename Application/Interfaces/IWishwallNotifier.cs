namespace Application.Interfaces
{
    public interface IWishwallNotifier
    {
        // Notify staff group of a new pending message
        Task NotifyNewPendingAsync(Guid eventId, Guid messageId, string message, string sentiment, DateTime createdAt);

        // Notify attendee group that a message was approved (shows on public wishwall)
        Task NotifyMessageApprovedAsync(Guid eventId, Guid messageId, string userName, string message, string sentiment, DateTime createdAt);

        // Notify LED group to display a message on screen
        Task NotifyLedDisplayAsync(Guid eventId, Guid messageId, string userName, string message, string sentiment, DateTime createdAt);

        // Notify the specific user that their message is pending moderation
        Task NotifyUserPendingAsync(string userId, Guid messageId, string message, DateTime createdAt);
    }
}
