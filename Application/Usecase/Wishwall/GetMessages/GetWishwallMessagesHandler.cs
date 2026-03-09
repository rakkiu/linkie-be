using Application.Interfaces;
using Domain.Interface;
using MediatR;

namespace Application.Usecase.Wishwall.GetMessages
{
    public class GetWishwallMessagesHandler : IRequestHandler<GetWishwallMessagesQuery, List<WishwallMessageDto>>
    {
        private readonly IWishwallRepository _repo;
        private readonly IEncryptionService _encryption;

        public GetWishwallMessagesHandler(IWishwallRepository repo, IEncryptionService encryption)
        {
            _repo = repo;
            _encryption = encryption;
        }

        public async Task<List<WishwallMessageDto>> Handle(GetWishwallMessagesQuery request, CancellationToken cancellationToken)
        {
            var messages = await _repo.GetByEventIdAsync(request.EventId, cancellationToken);

            return messages.Select(m => new WishwallMessageDto
            {
                Id = m.Id,
                UserName = m.User != null ? _encryption.Decrypt(m.User.Name) : "Anonymous",
                Message = m.Message,
                Sentiment = m.Sentiment.ToString(),
                CreatedAt = m.CreatedAt
            }).ToList();
        }
    }
}
