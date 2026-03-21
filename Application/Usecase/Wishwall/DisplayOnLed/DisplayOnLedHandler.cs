using Application.Interfaces;
using Domain.Interface;
using Application.Model.Admin;
using Application.Model.WishwallAi;
using MediatR;

namespace Application.Usecase.Wishwall.DisplayOnLed
{
    public class DisplayOnLedHandler : IRequestHandler<DisplayOnLedCommand, bool>
    {
        private readonly IWishwallRepository _repo;
        private readonly IWishwallNotifier _notifier;
        private readonly IEncryptionService _encryption;

        public DisplayOnLedHandler(
            IWishwallRepository repo,
            IWishwallNotifier notifier,
            IEncryptionService encryption)
        {
            _repo = repo;
            _notifier = notifier;
            _encryption = encryption;
        }

        public async Task<bool> Handle(DisplayOnLedCommand request, CancellationToken cancellationToken)
        {
            var message = await _repo.GetByIdAsync(request.MessageId, cancellationToken);
            if (message == null)
                throw new KeyNotFoundException("Message not found.");

            if (!message.IsApproved)
                throw new InvalidOperationException("Only approved messages can be displayed on the LED screen.");

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

            await _notifier.DisplayOnLedAsync(message.EventId, payload);

            return true;
        }
    }
}
