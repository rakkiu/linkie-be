using Application.Interfaces;
using Domain.Interface;
using Application.Model.Admin;
using Application.Model.WishwallAi;
using MediatR;

namespace Application.Usecase.Wishwall.ApproveMessage
{
    public class ApproveMessageHandler : IRequestHandler<ApproveMessageCommand, bool>
    {
        private readonly IWishwallRepository _repo;
        private readonly IWishwallNotifier _notifier;
        private readonly IEncryptionService _encryption;

        public ApproveMessageHandler(IWishwallRepository repo, IWishwallNotifier notifier, IEncryptionService encryption)
        {
            _repo = repo;
            _notifier = notifier;
            _encryption = encryption;
        }

        public async Task<bool> Handle(ApproveMessageCommand request, CancellationToken cancellationToken)
        {
            var message = await _repo.GetByIdAsync(request.MessageId, cancellationToken);
            if (message == null || message.EventId != request.EventId)
                throw new KeyNotFoundException("Message not found.");

            message.IsApproved = true;
            await _repo.SaveChangesAsync(cancellationToken);

            var userName = message.User != null ? _encryption.Decrypt(message.User.Name) : "Anonymous";

            await _notifier.BroadcastApprovedMessageAsync(message.EventId, new
            {
                id = message.Id,
                userName,
                message = message.Message,
                sentiment = message.Sentiment.ToString(),
                createdAt = message.CreatedAt
            });

            return true;
        }
    }
}
