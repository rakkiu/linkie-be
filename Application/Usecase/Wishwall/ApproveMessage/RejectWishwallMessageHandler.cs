using Domain.Interface;
using Domain.Enums;
using MediatR;

namespace Application.Usecase.Wishwall.ApproveMessage
{
    public class RejectWishwallMessageHandler : IRequestHandler<RejectWishwallMessageCommand, bool>
    {
        private readonly IWishwallRepository _repo;

        public RejectWishwallMessageHandler(IWishwallRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> Handle(RejectWishwallMessageCommand request, CancellationToken cancellationToken)
        {
            var message = await _repo.GetByIdAsync(request.MessageId, cancellationToken);
            if (message == null)
                throw new KeyNotFoundException("Message not found.");

            message.IsApproved = false;
            message.IsHidden = true; // Hide from public views
            message.Sentiment = WishwallSentiment.Negative; // Count as Negative
            
            await _repo.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
