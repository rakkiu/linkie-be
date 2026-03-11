using Application.Interfaces;
using Domain.Interface;
using MediatR;

namespace Application.Usecase.Wishwall.GetPendingMessages
{
    public class GetPendingWishwallMessagesHandler
        : IRequestHandler<GetPendingWishwallMessagesQuery, List<PendingWishwallMessageDto>>
    {
        private readonly IWishwallRepository _repo;
        private readonly IEncryptionService _encryption;

        public GetPendingWishwallMessagesHandler(IWishwallRepository repo, IEncryptionService encryption)
        {
            _repo = repo;
            _encryption = encryption;
        }

        public async Task<List<PendingWishwallMessageDto>> Handle(
            GetPendingWishwallMessagesQuery request,
            CancellationToken cancellationToken)
        {
            var messages = await _repo.GetPendingByEventIdAsync(request.EventId, cancellationToken);

            return messages.Select(m => new PendingWishwallMessageDto
            {
                Id = m.Id,
                UserId = m.UserId,
                UserName = m.User != null ? _encryption.Decrypt(m.User.Name) : "Anonymous",
                Message = m.Message,
                Sentiment = m.Sentiment.ToString(),
                CreatedAt = m.CreatedAt
            }).ToList();
        }
    }
}
