using Application.Interfaces;
using Domain.Interface;
using MediatR;

namespace Application.Usecase.Admin.Wishwall
{
    public class AdminWishwallMessageDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Sentiment { get; set; } = string.Empty;
        public bool IsHidden { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public record GetAdminWishwallQuery(Guid EventId, int Page = 1, int PageSize = 20)
        : IRequest<List<AdminWishwallMessageDto>>;

    public class GetAdminWishwallHandler : IRequestHandler<GetAdminWishwallQuery, List<AdminWishwallMessageDto>>
    {
        private readonly IAdminRepository _repo;
        private readonly IEncryptionService _encryption;

        public GetAdminWishwallHandler(IAdminRepository repo, IEncryptionService encryption)
        {
            _repo = repo;
            _encryption = encryption;
        }

        public async Task<List<AdminWishwallMessageDto>> Handle(GetAdminWishwallQuery request, CancellationToken cancellationToken)
        {
            var messages = await _repo.GetPagedWishwallMessagesAsync(
                request.EventId, request.Page, request.PageSize, cancellationToken);

            return messages.Select(m => new AdminWishwallMessageDto
            {
                Id = m.Id,
                UserName = m.User != null ? _encryption.Decrypt(m.User.Name) : "Anonymous",
                Message = m.Message,
                Sentiment = m.Sentiment.ToString().ToLower(),
                IsHidden = m.IsHidden,
                CreatedAt = m.CreatedAt
            }).ToList();
        }
    }
}
