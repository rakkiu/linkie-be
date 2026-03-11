using Application.Interfaces;
using Domain.Interface;
using MediatR;

namespace Application.Usecase.Wishwall.DisplayOnLed
{
    public class DisplayOnLedHandler : IRequestHandler<DisplayOnLedCommand, bool>
    {
        private readonly IWishwallRepository _repo;
        private readonly IWishwallNotifier _notifier;
        private readonly IEncryptionService _encryption;

        public DisplayOnLedHandler(IWishwallRepository repo, IWishwallNotifier notifier, IEncryptionService encryption)
        {
            _repo = repo;
            _notifier = notifier;
            _encryption = encryption;
        }

        public async Task<bool> Handle(DisplayOnLedCommand request, CancellationToken cancellationToken)
        {
            var message = await _repo.GetByIdAsync(request.MessageId, cancellationToken);
            if (message == null || message.EventId != request.EventId)
                throw new KeyNotFoundException("Message not found.");

            // Auto-approve if not already approved
            if (!message.IsApproved)
            {
                message.IsApproved = true;
                await _repo.SaveChangesAsync(cancellationToken);
            }

            var userName = message.User != null ? _encryption.Decrypt(message.User.Name) : "Anonymous";

            // Broadcast to LED screen group
            await _notifier.NotifyLedDisplayAsync(
                request.EventId, message.Id, userName,
                message.Message, message.Sentiment.ToString(), message.CreatedAt);

            return true;
        }
    }
}
