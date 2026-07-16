using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IWishwallAiModerationService
    {
        Task EnqueueModerationAsync(Guid messageId, string message, CancellationToken ct = default);
    }
}
