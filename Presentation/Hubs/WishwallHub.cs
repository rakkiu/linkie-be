using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Presentation.Hubs
{
    [Authorize]
    public class WishwallHub : Hub
    {
        // Staff joins a group to receive pending messages for an event
        public async Task JoinStaff(string eventId)
            => await Groups.AddToGroupAsync(Context.ConnectionId, $"staff-{eventId}");

        public async Task LeaveStaff(string eventId)
            => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"staff-{eventId}");

        // Attendees join to see approved messages on the wishwall feed
        public async Task JoinEvent(string eventId)
            => await Groups.AddToGroupAsync(Context.ConnectionId, $"event-{eventId}");

        public async Task LeaveEvent(string eventId)
            => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"event-{eventId}");

        // LED screen joins to receive display commands
        public async Task JoinLed(string eventId)
            => await Groups.AddToGroupAsync(Context.ConnectionId, $"led-{eventId}");

        public async Task LeaveLed(string eventId)
            => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"led-{eventId}");
    }
}
