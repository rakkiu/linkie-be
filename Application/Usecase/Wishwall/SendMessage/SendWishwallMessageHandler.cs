using Application.Interfaces;
using Domain.Entity;
using Domain.Enums;
using Domain.Interface;
using MediatR;

namespace Application.Usecase.Wishwall.SendMessage
{
    public class SendWishwallMessageHandler : IRequestHandler<SendWishwallMessageCommand, bool>
    {
        private readonly IWishwallRepository _repo;
        private readonly IEventRepository _eventRepo;
        private readonly IWishwallNotifier _notifier;

        public SendWishwallMessageHandler(IWishwallRepository repo, IEventRepository eventRepo, IWishwallNotifier notifier)
        {
            _repo = repo;
            _eventRepo = eventRepo;
            _notifier = notifier;
        }

        public async Task<bool> Handle(SendWishwallMessageCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                throw new ArgumentException("Message cannot be empty.");

            var ev = await _eventRepo.GetByIdAsync(request.EventId, cancellationToken);
            if (ev == null)
                throw new KeyNotFoundException("Event not found.");

            var sentiment = DetectSentiment(request.Message);

            var message = new WishwallMessage
            {
                EventId = request.EventId,
                UserId = request.UserId,
                Message = request.Message,
                Sentiment = sentiment,
                IsApproved = false,
                IsHidden = false
            };

            await _repo.AddAsync(message, cancellationToken);
            await _repo.SaveChangesAsync(cancellationToken);

            // Notify staff in real-time about new pending message
            await _notifier.NotifyNewPendingAsync(request.EventId, message.Id, message.Message, sentiment.ToString(), message.CreatedAt);
            // Notify the sender that their message is pending moderation
            await _notifier.NotifyUserPendingAsync(request.UserId.ToString(), message.Id, message.Message, message.CreatedAt);

            return true;
        }

        // Simple keyword-based sentiment detection
        private static WishwallSentiment DetectSentiment(string text)
        {
            var lower = text.ToLowerInvariant();

            string[] positiveWords = ["happy", "love", "great", "amazing", "congratulations", "congrats", "wonderful", "excellent", "thank", "vui", "tuyệt", "chúc mừng", "hạnh phúc", "tốt"];
            string[] negativeWords = ["sad", "bad", "terrible", "hate", "awful", "buồn", "tệ", "ghét", "khóc"];

            bool hasPositive = positiveWords.Any(w => lower.Contains(w));
            bool hasNegative = negativeWords.Any(w => lower.Contains(w));

            if (hasPositive && !hasNegative) return WishwallSentiment.Positive;
            if (hasNegative && !hasPositive) return WishwallSentiment.Negative;
            return WishwallSentiment.Neutral;
        }
    }
}
