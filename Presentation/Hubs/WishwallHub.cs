using Microsoft.AspNetCore.SignalR;

namespace Presentation.Hubs
{
    public class WishwallHub : Hub
    {
        // Client calls this to subscribe to approved messages for a given event
        public async Task JoinEvent(string eventId)
            => await Groups.AddToGroupAsync(Context.ConnectionId, $"event:{eventId}");

        public async Task LeaveEvent(string eventId)
            => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"event:{eventId}");

        // Staff joins a staff-only channel to receive pending-review notifications
        public async Task JoinStaff(string eventId)
            => await Groups.AddToGroupAsync(Context.ConnectionId, $"staff:{eventId}");

        public async Task LeaveStaff(string eventId)
            => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"staff:{eventId}");

        // LED screen joins a dedicated group; receives LedDisplay events pushed by organizer
        public async Task JoinLed(string eventId)
            => await Groups.AddToGroupAsync(Context.ConnectionId, $"led:{eventId}");

        public async Task LeaveLed(string eventId)
            => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"led:{eventId}");
    }
}
