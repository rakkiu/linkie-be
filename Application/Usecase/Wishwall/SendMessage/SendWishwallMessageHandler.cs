using Application.Interfaces;
using Domain.Entity;
using Domain.Enums;
using Domain.Interface;
using Application.Model.Admin;
using Application.Model.WishwallAi;
using Domain.Interfaces;
using MediatR;

namespace Application.Usecase.Wishwall.SendMessage
{
    public class SendWishwallMessageHandler : IRequestHandler<SendWishwallMessageCommand, bool>
    {
        private readonly IWishwallRepository _repo;
        private readonly IEventRepository _eventRepo;
        private readonly IWishwallNotifier _notifier;
        private readonly IUserRepository _userRepo;
        private readonly IEncryptionService _encryption;
        private readonly IWishwallAiModerationService _aiModeration;

        public SendWishwallMessageHandler(
            IWishwallRepository repo,
            IEventRepository eventRepo,
            IWishwallNotifier notifier,
            IUserRepository userRepo,
            IEncryptionService encryption,
            IWishwallAiModerationService aiModeration)
        {
            _repo = repo;
            _eventRepo = eventRepo;
            _notifier = notifier;
            _userRepo = userRepo;
            _encryption = encryption;
            _aiModeration = aiModeration;
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

            _ = _aiModeration.EnqueueModerationAsync(message.Id, message.Message, cancellationToken);

            // Notify the sender their message is queued for approval
            await _notifier.NotifyMessagePendingAsync(request.UserId, new
            {
                id = message.Id,
                message = message.Message,
                createdAt = message.CreatedAt
            });

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
