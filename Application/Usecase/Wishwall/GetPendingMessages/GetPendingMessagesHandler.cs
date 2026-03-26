using Application.Interfaces;
using Domain.Interface;
using Application.Model.Admin;
using Application.Model.WishwallAi;
using MediatR;

namespace Application.Usecase.Wishwall.GetPendingMessages
{
    public class GetPendingMessagesHandler : IRequestHandler<GetPendingMessagesQuery, List<PendingMessageDto>>
    {
        private readonly IWishwallRepository _repo;
        private readonly IEncryptionService _encryption;

        public GetPendingMessagesHandler(IWishwallRepository repo, IEncryptionService encryption)
        {
            _repo = repo;
            _encryption = encryption;
        }

        public async Task<List<PendingMessageDto>> Handle(GetPendingMessagesQuery request, CancellationToken cancellationToken)
        {
            var messages = await _repo.GetPendingByEventIdAsync(request.EventId, cancellationToken);

            return messages.Select(m => new PendingMessageDto
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
