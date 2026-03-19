namespace Application.Interfaces
{
    public interface IWishwallNotifier
    {
        // Notify the sender that their message is pending approval
        Task NotifyMessagePendingAsync(Guid userId, object payload);

        // Broadcast an approved message to all viewers of the event
        Task BroadcastApprovedMessageAsync(Guid eventId, object payload);

        // Notify staff that a new message is pending review
        Task NotifyStaffNewPendingAsync(Guid eventId, object payload);

        // Notify staff that a new AI Log has been added (real-time)
        Task NotifyStaffNewAiLogAsync(Guid eventId, object payload);

        // Push a specific message to the LED screen group
        Task DisplayOnLedAsync(Guid eventId, object payload);
    }
}
