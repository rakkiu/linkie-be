using Application.Interfaces;
using Domain.Interface;
using MediatR;

namespace Application.Usecase.Wishwall.ApproveMessage
{
    public class ApproveWishwallMessageHandler : IRequestHandler<ApproveWishwallMessageCommand, bool>
    {
        private readonly IWishwallRepository _repo;
        private readonly IWishwallNotifier _notifier;
        private readonly IEncryptionService _encryption;

        public ApproveWishwallMessageHandler(
            IWishwallRepository repo,
            IWishwallNotifier notifier,
            IEncryptionService encryption)
        {
            _repo = repo;
            _notifier = notifier;
            _encryption = encryption;
        }

        public async Task<bool> Handle(ApproveWishwallMessageCommand request, CancellationToken cancellationToken)
        {
            var message = await _repo.GetByIdAsync(request.MessageId, cancellationToken);
            if (message == null)
                throw new KeyNotFoundException("Message not found.");

            if (message.IsApproved)
                return true; // already approved — idempotent

            message.IsApproved = true;
            await _repo.SaveChangesAsync(cancellationToken);

            var userName = message.User != null
                ? _encryption.Decrypt(message.User.Name)
                : "Anonymous";

            var payload = new
            {
                id = message.Id,
                userName,
                message = message.Message,
                sentiment = message.Sentiment.ToString(),
                createdAt = message.CreatedAt
            };

            await _notifier.BroadcastApprovedMessageAsync(message.EventId, payload);

            return true;
        }
    }
}
