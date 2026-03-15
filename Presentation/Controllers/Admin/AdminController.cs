using Application.Usecase.Admin.Dashboard;
using Application.Usecase.Admin.FrameAnalytics;
using Application.Usecase.Admin.Sentiment;
using Application.Usecase.Admin.SystemHealth;
using Application.Usecase.Admin.Wishwall;
using Application.Usecase.ArFrame.DeleteFrame;
using Application.Usecase.ArFrame.GetFrames;
using Application.Usecase.ArFrame.ToggleFrame;
using Application.Usecase.ArFrame.UploadFrame;
using Application.Usecase.EventManagement.CreateEvent;
using Application.Usecase.EventManagement.DeleteEvent;
using Application.Usecase.EventManagement.GetAdminEventList;
using Application.Usecase.EventManagement.UpdateEvent;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Presentation.Common;

namespace Presentation.Controllers.Admin
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICloudinaryService _cloudinary;

        public AdminController(IMediator mediator, ICloudinaryService cloudinary)
        {
            _mediator = mediator;
            _cloudinary = cloudinary;
        }

        // ──────────────────────────────────────────────────────────────
        // EVENT MANAGEMENT
        // ──────────────────────────────────────────────────────────────

        // GET /api/admin/events
        [HttpGet("events")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminEventDto>>), 200)]
        public async Task<ActionResult<ApiResponse<List<AdminEventDto>>>> GetAdminEvents(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAdminEventListQuery(), ct);
            return Ok(new ApiResponse<List<AdminEventDto>>
            {
                StatusCode = 200,
                Message = "Admin event list retrieved successfully.",
                Data = result,
                ResponsedAt = DateTime.UtcNow
            });
        }

        // POST /api/admin/events
        [HttpPost("events")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<CreateEventResult>), 201)]
        public async Task<ActionResult<ApiResponse<CreateEventResult>>> CreateEvent(
            [FromForm] CreateEventRequest req,
            IFormFile? thumbnail,
            CancellationToken ct)
        {
            string? thumbnailUrl = req.ThumbnailUrl;
            if (thumbnail != null && thumbnail.Length > 0)
            {
                await using var stream = thumbnail.OpenReadStream();
                thumbnailUrl = await _cloudinary.UploadImageAsync(stream, thumbnail.FileName, ct);
            }

            var command = new CreateEventCommand(req.Name, req.Description, req.StartTime, req.EndTime, req.Location, req.MaxParticipants, req.IsWishwallEnabled, req.Status, thumbnailUrl);
            var result = await _mediator.Send(command, ct);
            return StatusCode(201, new ApiResponse<CreateEventResult>
            {
                StatusCode = 201,
                Message = "Event created successfully.",
                Data = result,
                ResponsedAt = DateTime.UtcNow
            });
        }

        // PUT /api/admin/events/{id}
        [HttpPut("events/{id:guid}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<UpdateEventResult>), 200)]
        public async Task<ActionResult<ApiResponse<UpdateEventResult>>> UpdateEvent(
            Guid id,
            [FromForm] UpdateEventRequest req,
            IFormFile? thumbnail,
            CancellationToken ct)
        {
            string? thumbnailUrl = req.ThumbnailUrl;
            if (thumbnail != null && thumbnail.Length > 0)
            {
                await using var stream = thumbnail.OpenReadStream();
                thumbnailUrl = await _cloudinary.UploadImageAsync(stream, thumbnail.FileName, ct);
            }

            var command = new UpdateEventCommand(id, req.Name, req.Description, req.StartTime, req.EndTime, req.Location, req.MaxParticipants, req.IsWishwallEnabled, req.Status, thumbnailUrl);
            var result = await _mediator.Send(command, ct);
            return Ok(new ApiResponse<UpdateEventResult>
            {
                StatusCode = 200,
                Message = "Event updated successfully.",
                Data = result,
                ResponsedAt = DateTime.UtcNow
            });
        }

        // DELETE /api/admin/events/{id}
        [HttpDelete("events/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteEvent(Guid id, CancellationToken ct)
        {
            await _mediator.Send(new DeleteEventCommand(id), ct);
            return Ok(new ApiResponse<object>
            {
                StatusCode = 200,
                Message = "Event deleted successfully.",
                Data = null,
                ResponsedAt = DateTime.UtcNow
            });
        }

        // FR-01: GET /api/admin/events/{eventId}/dashboard
        [HttpGet("events/{eventId:guid}/dashboard")]
        [ProducesResponseType(typeof(ApiResponse<EventDashboardDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<EventDashboardDto>>> GetDashboard(
            Guid eventId,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _mediator.Send(new GetEventDashboardQuery(eventId), cancellationToken);
                return Ok(new ApiResponse<EventDashboardDto>
                {
                    StatusCode = 200,
                    Message = "Dashboard data retrieved",
                    Data = result,
                    ResponsedAt = DateTime.UtcNow
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    StatusCode = 500,
                    Message = "An error occurred while retrieving dashboard data.",
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
        }

        // FR-02: GET /api/admin/events/{eventId}/analytics/sentiment?interval=minute|hour
        [HttpGet("events/{eventId:guid}/analytics/sentiment")]
        [ProducesResponseType(typeof(ApiResponse<List<SentimentDataPointDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<List<SentimentDataPointDto>>>> GetSentimentAnalytics(
            Guid eventId,
            [FromQuery] string interval = "minute",
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _mediator.Send(
                    new GetSentimentAnalyticsQuery(eventId, interval), cancellationToken);

                return Ok(new ApiResponse<List<SentimentDataPointDto>>
                {
                    StatusCode = 200,
                    Message = "Sentiment analytics retrieved",
                    Data = result,
                    ResponsedAt = DateTime.UtcNow
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    StatusCode = 500,
                    Message = "An error occurred while retrieving sentiment analytics.",
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
        }

        // FR-03: GET /api/admin/events/{eventId}/analytics/frame-usage
        [HttpGet("events/{eventId:guid}/analytics/frame-usage")]
        [ProducesResponseType(typeof(ApiResponse<FrameUsageAnalyticsDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<FrameUsageAnalyticsDto>>> GetFrameUsageAnalytics(
            Guid eventId,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _mediator.Send(new GetFrameUsageAnalyticsQuery(eventId), cancellationToken);
                return Ok(new ApiResponse<FrameUsageAnalyticsDto>
                {
                    StatusCode = 200,
                    Message = "Frame usage analytics retrieved",
                    Data = result,
                    ResponsedAt = DateTime.UtcNow
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    StatusCode = 500,
                    Message = "An error occurred while retrieving frame usage analytics.",
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
        }

        // FR-04: GET /api/admin/events/{eventId}/wishwall/messages?page=1&pageSize=20
        [HttpGet("events/{eventId:guid}/wishwall/messages")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminWishwallMessageDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<List<AdminWishwallMessageDto>>>> GetWishwallMessages(
            Guid eventId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _mediator.Send(
                    new GetAdminWishwallQuery(eventId, page, pageSize), cancellationToken);

                return Ok(new ApiResponse<List<AdminWishwallMessageDto>>
                {
                    StatusCode = 200,
                    Message = "Wishwall messages retrieved",
                    Data = result,
                    ResponsedAt = DateTime.UtcNow
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    StatusCode = 500,
                    Message = "An error occurred while retrieving wishwall messages.",
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
        }

        // FR-05: GET /api/admin/system/health
        [HttpGet("system/health")]
        [ProducesResponseType(typeof(ApiResponse<SystemHealthDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<SystemHealthDto>>> GetSystemHealth(
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _mediator.Send(new GetSystemHealthQuery(), cancellationToken);
                return Ok(new ApiResponse<SystemHealthDto>
                {
                    StatusCode = 200,
                    Message = "System status retrieved",
                    Data = result,
                    ResponsedAt = DateTime.UtcNow
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    StatusCode = 500,
                    Message = "An error occurred while retrieving system health.",
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
        }
        // ──────────────────────────────────────────────────────────────
        // AR FRAME MANAGEMENT
        // ──────────────────────────────────────────────────────────────

        // GET /api/admin/events/{eventId}/frames
        [HttpGet("events/{eventId:guid}/frames")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminArFrameDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<List<AdminArFrameDto>>>> GetAllArFrames(
            Guid eventId,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _mediator.Send(new GetAllArFramesQuery(eventId), cancellationToken);
                return Ok(new ApiResponse<List<AdminArFrameDto>>
                {
                    StatusCode = 200,
                    Message = "AR frames retrieved successfully.",
                    Data = result,
                    ResponsedAt = DateTime.UtcNow
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    StatusCode = 500,
                    Message = "An error occurred while retrieving AR frames.",
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
        }

        // POST /api/admin/events/{eventId}/frames
        [HttpPost("events/{eventId:guid}/frames")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<UploadArFrameResult>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<UploadArFrameResult>>> UploadArFrame(
            Guid eventId,
            [FromForm] string frameName,
            IFormFile file,
            CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(frameName))
                    return BadRequest(new ApiResponse<object>
                    {
                        StatusCode = 400,
                        Message = "frameName is required.",
                        Data = null,
                        ResponsedAt = DateTime.UtcNow
                    });

                var result = await _mediator.Send(
                    new UploadArFrameCommand(eventId, frameName, file), cancellationToken);

                return StatusCode(201, new ApiResponse<UploadArFrameResult>
                {
                    StatusCode = 201,
                    Message = "AR frame uploaded successfully.",
                    Data = result,
                    ResponsedAt = DateTime.UtcNow
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    StatusCode = 500,
                    Message = "An error occurred while uploading the AR frame.",
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
        }

        // PATCH /api/admin/frames/{frameId}/toggle
        [HttpPatch("frames/{frameId:guid}/toggle")]
        [ProducesResponseType(typeof(ApiResponse<ToggleArFrameResult>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<ToggleArFrameResult>>> ToggleArFrame(
            Guid frameId,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _mediator.Send(new ToggleArFrameCommand(frameId), cancellationToken);
                return Ok(new ApiResponse<ToggleArFrameResult>
                {
                    StatusCode = 200,
                    Message = $"AR frame is now {(result.IsActive ? "active" : "inactive")}.",
                    Data = result,
                    ResponsedAt = DateTime.UtcNow
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    StatusCode = 404,
                    Message = ex.Message,
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    StatusCode = 500,
                    Message = "An error occurred while toggling the AR frame.",
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
        }

        // DELETE /api/admin/frames/{frameId}
        [HttpDelete("frames/{frameId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteArFrame(
            Guid frameId,
            CancellationToken cancellationToken)
        {
            try
            {
                await _mediator.Send(new DeleteArFrameCommand(frameId), cancellationToken);
                return Ok(new ApiResponse<object>
                {
                    StatusCode = 200,
                    Message = "AR frame deleted successfully.",
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    StatusCode = 404,
                    Message = ex.Message,
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    StatusCode = 500,
                    Message = "An error occurred while deleting the AR frame.",
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
        }
    }
}
