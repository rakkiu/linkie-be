using Application.Usecase.Admin.Dashboard;
using Application.Usecase.Admin.FrameAnalytics;
using Application.Usecase.Admin.Sentiment;
using Application.Usecase.Admin.SystemHealth;
using Application.Usecase.Admin.Wishwall;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Common;

namespace Presentation.Controllers.Admin
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator) => _mediator = mediator;

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
    }
}
