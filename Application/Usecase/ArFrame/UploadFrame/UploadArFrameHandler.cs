using Application.Interfaces;
using Domain.Entity;
using Domain.Interface;
using Application.Model.Admin;
using Application.Model.WishwallAi;
using MediatR;

namespace Application.Usecase.ArFrame.UploadFrame
{
    public class UploadArFrameHandler : IRequestHandler<UploadArFrameCommand, UploadArFrameResult>
    {
        private readonly IArFrameRepository _repo;
        private readonly ICloudinaryService _cloudinary;

        public UploadArFrameHandler(IArFrameRepository repo, ICloudinaryService cloudinary)
        {
            _repo = repo;
            _cloudinary = cloudinary;
        }

        public async Task<UploadArFrameResult> Handle(UploadArFrameCommand request, CancellationToken cancellationToken)
        {
            if (request.File == null || request.File.Length == 0)
                throw new ArgumentException("File is required and must not be empty.");

            // 1. Upload to Cloudinary
            await using var stream = request.File.OpenReadStream();
            var frameUrl = await _cloudinary.UploadImageAsync(stream, request.File.FileName, cancellationToken);

            // 2. Save to database
            var frame = new Domain.Entity.ArFrame
            {
                EventId = request.EventId,
                FrameName = request.FrameName,
                FrameUrl = frameUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(frame, cancellationToken);
            await _repo.SaveChangesAsync(cancellationToken);

            return new UploadArFrameResult(frame.Id, frame.FrameName, frame.FrameUrl);
        }
    }
}
