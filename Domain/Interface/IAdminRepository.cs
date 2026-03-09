using Domain.Entity;
using Domain.Enums;

namespace Domain.Interface
{
    public interface IAdminRepository
    {
        // Dashboard (FR-01)
        Task<int> GetParticipantCountAsync(Guid eventId, CancellationToken ct = default);
        Task<int> GetMessageCountAsync(Guid eventId, CancellationToken ct = default);
        Task<int> GetFrameUsageCountAsync(Guid eventId, CancellationToken ct = default);
        Task<Dictionary<WishwallSentiment, int>> GetSentimentSummaryAsync(Guid eventId, CancellationToken ct = default);
        Task<(Guid FrameId, string FrameName, int Usage)?> GetTopFrameAsync(Guid eventId, CancellationToken ct = default);

        // Sentiment analytics (FR-02)
        Task<List<WishwallMessage>> GetMessagesWithCreatedAtAsync(Guid eventId, CancellationToken ct = default);

        // Frame usage analytics (FR-03)
        Task<List<(Guid FrameId, string FrameName, int Usage)>> GetFrameStatsAsync(Guid eventId, CancellationToken ct = default);
        Task<int> GetActiveFrameCountAsync(Guid eventId, CancellationToken ct = default);

        // Admin wishwall — includes hidden messages (FR-04)
        Task<List<WishwallMessage>> GetPagedWishwallMessagesAsync(Guid eventId, int page, int pageSize, CancellationToken ct = default);
        Task<int> GetTotalWishwallCountAsync(Guid eventId, CancellationToken ct = default);

        // System health ping (FR-05)
        Task<bool> PingAsync(CancellationToken ct = default);
    }
}
