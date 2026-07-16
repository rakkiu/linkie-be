using Domain.Interface;
using Application.Interfaces;
using Application.Model.Admin;
using Application.Model.WishwallAi;
using Domain.Enums;
using Domain.Entity;
using MediatR;

namespace Application.Usecase.Wishwall.ApproveMessage
{
    public class RejectWishwallMessageHandler : IRequestHandler<RejectWishwallMessageCommand, bool>
    {
        private readonly IWishwallRepository _repo;
        private readonly IWishwallNotifier _notifier;

        public RejectWishwallMessageHandler(IWishwallRepository repo, IWishwallNotifier notifier)
        {
            _repo = repo;
            _notifier = notifier;
        }

        public async Task<bool> Handle(RejectWishwallMessageCommand request, CancellationToken cancellationToken)
        {
            var message = await _repo.GetByIdAsync(request.MessageId, cancellationToken);
            if (message == null)
                throw new KeyNotFoundException("Message not found.");

            message.IsApproved = false;
            message.IsHidden = true; // Hide from public views
            message.Sentiment = WishwallSentiment.Negative; // Count as Negative

            // Log staff rejection to AI Logs
            await _repo.AddAiLogAsync(new WishwallAiLog
            {
                MessageId = request.MessageId,
                Label = "BLOCK",
                Reason = "Từ chối bởi Staff",
                Source = "staff",
                DurationMs = 0
            }, cancellationToken);

            // Notify Staff via SignalR (Real-time)
            await _notifier.NotifyStaffNewAiLogAsync(message.EventId, new
            {
                messageId = request.MessageId,
                message = message.Message,
                label = "BLOCK",
                reason = "Từ chối bởi Staff",
                source = "staff",
                createdAt = DateTime.UtcNow
            });
            
            await _repo.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
