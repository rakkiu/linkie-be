using Application.Usecase.Admin.Dashboard;
using Application.Usecase.Admin.Report;
using Application.Usecase.ArFrame.GetFrames;
using Application.Usecase.ArFrame.UploadFrame;
using Application.Usecase.ArFrame.ToggleFrame;
using Application.Usecase.ArFrame.DeleteFrame;
using Application.Usecase.Admin.Wishwall;
using Application.Usecase.Wishwall.ApproveMessage;
using Domain.Interface;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Presentation.Common;
using Domain.Enums;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/b2b")]
    [Authorize(Roles = "Organizer,Admin")]
    public class B2BController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IArFrameRepository _frameRepo;
        private readonly IWishwallRepository _wishwallRepo;

        public B2BController(IMediator mediator, IArFrameRepository frameRepo, IWishwallRepository wishwallRepo)
        {
            _mediator = mediator;
            _frameRepo = frameRepo;
            _wishwallRepo = wishwallRepo;
        }

        [HttpGet("events/{eventId:guid}/dashboard-summary")]
        public async Task<IActionResult> GetDashboardSummary(Guid eventId, CancellationToken ct)
        {
            if (User.IsInRole("Organizer"))
            {
                var managedEventIdClaim = User.FindFirst("managed_event_id")?.Value;
                if (string.IsNullOrEmpty(managedEventIdClaim) || !Guid.TryParse(managedEventIdClaim, out var managedEventId) || managedEventId != eventId)
                {
                    return Forbid();
                }
            }

            var result = await _mediator.Send(new GetDashboardSummaryQuery(eventId), ct);
            return Ok(new ApiResponse<DashboardSummaryDto>
            {
                StatusCode = 200,
                Message = "Dashboard summary retrieved successfully.",
                Data = result,
                ResponsedAt = DateTime.UtcNow
            });
        }
        [HttpGet("events/{eventId:guid}/report")]
        public async Task<IActionResult> GetEventReport(Guid eventId, CancellationToken ct)
        {
            if (User.IsInRole("Organizer"))
            {
                var managedEventIdClaim = User.FindFirst("managed_event_id")?.Value;
                if (string.IsNullOrEmpty(managedEventIdClaim) || !Guid.TryParse(managedEventIdClaim, out var managedEventId) || managedEventId != eventId)
                {
                    return Forbid();
                }
            }

            var result = await _mediator.Send(new GetEventReportQuery(eventId), ct);
            return Ok(new ApiResponse<EventReportDto>
            {
                StatusCode = 200,
                Message = "Event report retrieved successfully.",
                Data = result,
                ResponsedAt = DateTime.UtcNow
            });
        }

        private bool IsAuthorizedForEvent(Guid eventId)
        {
            if (User.IsInRole("Organizer"))
            {
                var managedEventIdClaim = User.FindFirst("managed_event_id")?.Value;
                if (string.IsNullOrEmpty(managedEventIdClaim) || !Guid.TryParse(managedEventIdClaim, out var managedEventId) || managedEventId != eventId)
                {
                    return false;
                }
            }
            return true;
        }

        // GET /api/b2b/events/{eventId}/frames
        [HttpGet("events/{eventId:guid}/frames")]
        public async Task<IActionResult> GetAllArFrames(Guid eventId, CancellationToken ct)
        {
            if (!IsAuthorizedForEvent(eventId)) return Forbid();

            var result = await _mediator.Send(new GetAllArFramesQuery(eventId), ct);
            return Ok(new ApiResponse<List<AdminArFrameDto>>
            {
                StatusCode = 200,
                Message = "AR frames retrieved successfully.",
                Data = result,
                ResponsedAt = DateTime.UtcNow
            });
        }

        // POST /api/b2b/events/{eventId}/frames
        [HttpPost("events/{eventId:guid}/frames")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadArFrame(Guid eventId, [FromForm] string frameName, IFormFile file, CancellationToken ct)
        {
            if (!IsAuthorizedForEvent(eventId)) return Forbid();

            if (string.IsNullOrWhiteSpace(frameName))
                return BadRequest("frameName is required.");

            var result = await _mediator.Send(new UploadArFrameCommand(eventId, frameName, file), ct);
            return Ok(new ApiResponse<UploadArFrameResult>
            {
                StatusCode = 201,
                Message = "AR frame uploaded successfully.",
                Data = result,
                ResponsedAt = DateTime.UtcNow
            });
        }

        // PATCH /api/b2b/frames/{frameId}/toggle
        [HttpPatch("frames/{frameId:guid}/toggle")]
        public async Task<IActionResult> ToggleArFrame(Guid frameId, CancellationToken ct)
        {
            var frame = await _frameRepo.GetByIdAsync(frameId, ct);
            if (frame == null) return NotFound("Frame not found.");
            if (!IsAuthorizedForEvent(frame.EventId)) return Forbid();

            var result = await _mediator.Send(new ToggleArFrameCommand(frameId), ct);
            return Ok(new ApiResponse<ToggleArFrameResult>
            {
                StatusCode = 200,
                Message = $"AR frame is now {(result.IsActive ? "active" : "inactive")}.",
                Data = result,
                ResponsedAt = DateTime.UtcNow
            });
        }

        // DELETE /api/b2b/frames/{frameId}
        [HttpDelete("frames/{frameId:guid}")]
        public async Task<IActionResult> DeleteArFrame(Guid frameId, CancellationToken ct)
        {
            var frame = await _frameRepo.GetByIdAsync(frameId, ct);
            if (frame == null) return NotFound("Frame not found.");
            if (!IsAuthorizedForEvent(frame.EventId)) return Forbid();

            await _mediator.Send(new DeleteArFrameCommand(frameId), ct);
            return Ok(new ApiResponse<object>
            {
                StatusCode = 200,
                Message = "AR frame deleted successfully.",
                Data = null,
                ResponsedAt = DateTime.UtcNow
            });
        }

        // GET /api/b2b/events/{eventId}/wishwall/messages
        [HttpGet("events/{eventId:guid}/wishwall/messages")]
        public async Task<IActionResult> GetWishwallMessages(Guid eventId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        {
            if (!IsAuthorizedForEvent(eventId)) return Forbid();

            var result = await _mediator.Send(new GetAdminWishwallQuery(eventId, page, pageSize), ct);
            return Ok(new ApiResponse<List<AdminWishwallMessageDto>>
            {
                StatusCode = 200,
                Message = "Wishwall messages retrieved.",
                Data = result,
                ResponsedAt = DateTime.UtcNow
            });
        }

        // PATCH /api/b2b/wishwall/{messageId}/approve
        [HttpPatch("wishwall/{messageId:guid}/approve")]
        public async Task<IActionResult> ApproveWishwallMessage(Guid messageId, [FromQuery] WishwallSentiment sentiment = WishwallSentiment.Neutral, CancellationToken ct = default)
        {
            var message = await _wishwallRepo.GetByIdAsync(messageId, ct);
            if (message == null) return NotFound("Message not found.");
            if (!IsAuthorizedForEvent(message.EventId)) return Forbid();

            await _mediator.Send(new ApproveWishwallMessageCommand(messageId, sentiment), ct);
            return Ok(new ApiResponse<object>
            {
                StatusCode = 200,
                Message = "Message approved successfully.",
                Data = null,
                ResponsedAt = DateTime.UtcNow
            });
        }

        // PATCH /api/b2b/wishwall/{messageId}/reject
        [HttpPatch("wishwall/{messageId:guid}/reject")]
        public async Task<IActionResult> RejectWishwallMessage(Guid messageId, CancellationToken ct = default)
        {
            var message = await _wishwallRepo.GetByIdAsync(messageId, ct);
            if (message == null) return NotFound("Message not found.");
            if (!IsAuthorizedForEvent(message.EventId)) return Forbid();

            await _mediator.Send(new RejectWishwallMessageCommand(messageId), ct);
            return Ok(new ApiResponse<object>
            {
                StatusCode = 200,
                Message = "Message rejected successfully.",
                Data = null,
                ResponsedAt = DateTime.UtcNow
            });
        }
    }
}
