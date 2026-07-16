using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Usecase.ArFrame.UploadFrame
{
    public record UploadArFrameCommand(
        Guid EventId,
        string FrameName,
        IFormFile File
    ) : IRequest<UploadArFrameResult>;

    public record UploadArFrameResult(Guid Id, string FrameName, string FrameUrl);
}
