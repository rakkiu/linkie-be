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

        public SendWishwallMessageHandler(IWishwallRepository repo, IEventRepository eventRepo)
        {
            _repo = repo;
            _eventRepo = eventRepo;
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
                IsHidden = false
            };

            await _repo.AddAsync(message, cancellationToken);
            await _repo.SaveChangesAsync(cancellationToken);
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
