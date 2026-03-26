using Application.Usecase.ArFrame.GetFrames;
using Application.Usecase.ArFrame.RecordUsage;
using Application.Usecase.EventManagement.GetEventById;
using Application.Usecase.EventManagement.GetEvents;
using Application.Usecase.Wishwall.ApproveMessage;
using Application.Usecase.Wishwall.DisplayOnLed;
using Application.Usecase.Wishwall.GetMessages;
using Application.Usecase.Wishwall.GetPendingMessages;
using Application.Usecase.Wishwall.SendMessage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Common;
using Domain.Enums;
using System.Security.Claims;

namespace Presentation.Controllers.Event
{
    [Route("api/events")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EventController(IMediator mediator) => _mediator = mediator;

        // GET /api/events?status=Active|Draft|Completed|Cancelled
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<EventResponseDto>>), 200)]
        public async Task<ActionResult<ApiResponse<List<EventResponseDto>>>> GetEvents(
            [FromQuery] string? status,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetEventsQuery(status), cancellationToken);
            return Ok(new ApiResponse<List<EventResponseDto>>
            {
                StatusCode = 200,
                Message = "Events retrieved successfully.",
                Data = result,
                ResponsedAt = DateTime.UtcNow
            });
        }

        // GET /api/events/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EventDetailDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<ActionResult<ApiResponse<EventDetailDto>>> GetEventById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetEventByIdQuery(id), cancellationToken);
            if (result == null)
                return NotFound(new ApiResponse<object>
                {
                    StatusCode = 404,
                    Message = $"Event with ID {id} was not found.",
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });

            return Ok(new ApiResponse<EventDetailDto>
            {
                StatusCode = 200,
                Message = "Event retrieved successfully.",
                Data = result,
                ResponsedAt = DateTime.UtcNow
            });
        }

        // GET /api/events/{eventId}/wishwall
        [HttpGet("{eventId:guid}/wishwall")]
        [ProducesResponseType(typeof(ApiResponse<List<WishwallMessageDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<List<WishwallMessageDto>>>> GetWishwallMessages(
            Guid eventId,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _mediator.Send(new GetWishwallMessagesQuery(eventId), cancellationToken);
                return Ok(new ApiResponse<List<WishwallMessageDto>>
                {
                    StatusCode = 200,
                    Message = "Wishwall messages retrieved successfully.",
                    Data = result,
                    ResponsedAt = DateTime.UtcNow
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    StatusCode = 500,
                    Message = "An error occurred while retrieving messages.",
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
        }

        // POST /api/events/{eventId}/wishwall
        [Authorize]
        [HttpPost("{eventId:guid}/wishwall")]
        [ProducesResponseType(typeof(ApiResponse<object>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<object>>> SendWishwallMessage(
            Guid eventId,
            [FromBody] SendWishwallMessageRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        StatusCode = 401,
                        Message = "Unauthorized. Please log in again.",
                        Data = null,
                        ResponsedAt = DateTime.UtcNow
                    });
                }

                await _mediator.Send(new SendWishwallMessageCommand(eventId, userId, request.Message), cancellationToken);

                return StatusCode(201, new ApiResponse<object>
                {
                    StatusCode = 201,
                    Message = "Message sent successfully.",
                    Data = null,
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
                    Message = "An error occurred while sending the message.",
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
        }

        // PATCH /api/events/{eventId}/wishwall/{messageId}/approve?sentiment=Positive|Neutral
        [Authorize(Roles = "Staff,Admin")]
        [HttpPatch("{eventId:guid}/wishwall/{messageId:guid}/approve")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<object>>> ApproveWishwallMessage(
            Guid eventId,
            Guid messageId,
            [FromQuery] WishwallSentiment sentiment = WishwallSentiment.Neutral,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _mediator.Send(new ApproveWishwallMessageCommand(messageId, sentiment), cancellationToken);

                return Ok(new ApiResponse<object>
                {
                    StatusCode = 200,
                    Message = "Message approved successfully.",
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
                    Message = "An error occurred while approving the message.",
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
        }

        // PATCH /api/events/{eventId}/wishwall/{messageId}/reject
        [Authorize(Roles = "Staff,Admin")]
        [HttpPatch("{eventId:guid}/wishwall/{messageId:guid}/reject")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<object>>> RejectWishwallMessage(
            Guid eventId,
            Guid messageId,
            CancellationToken cancellationToken)
        {
            try
            {
                await _mediator.Send(new RejectWishwallMessageCommand(messageId), cancellationToken);

                return Ok(new ApiResponse<object>
                {
                    StatusCode = 200,
                    Message = "Message rejected successfully.",
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
                    Message = "An error occurred while rejecting the message.",
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
        }

        // GET /api/events/{eventId}/wishwall/pending
        [Authorize(Roles = "Staff,Admin")]
        [HttpGet("{eventId:guid}/wishwall/pending")]
        [ProducesResponseType(typeof(ApiResponse<List<PendingWishwallMessageDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<List<PendingWishwallMessageDto>>>> GetPendingWishwallMessages(
            Guid eventId,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _mediator.Send(new GetPendingWishwallMessagesQuery(eventId), cancellationToken);
                return Ok(new ApiResponse<List<PendingWishwallMessageDto>>
                {
                    StatusCode = 200,
                    Message = "Pending wishwall messages retrieved successfully.",
                    Data = result,
                    ResponsedAt = DateTime.UtcNow
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    StatusCode = 500,
                    Message = "An error occurred while retrieving pending messages.",
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
        }

        // POST /api/events/{eventId}/wishwall/{messageId}/display
        [Authorize(Roles = "Staff,Admin")]
        [HttpPost("{eventId:guid}/wishwall/{messageId:guid}/display")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<object>>> DisplayWishwallOnLed(
            Guid eventId,
            Guid messageId,
            CancellationToken cancellationToken)
        {
            try
            {
                await _mediator.Send(new DisplayOnLedCommand(messageId), cancellationToken);

                return Ok(new ApiResponse<object>
                {
                    StatusCode = 200,
                    Message = "Message pushed to LED screen.",
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
            catch (InvalidOperationException ex)
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
                    Message = "An error occurred while pushing the message to LED.",
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
        }

        // GET /api/events/{eventId}/frames
        [HttpGet("{eventId:guid}/frames")]
        [ProducesResponseType(typeof(ApiResponse<List<ArFrameDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<List<ArFrameDto>>>> GetArFrames(
            Guid eventId,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _mediator.Send(new GetArFramesQuery(eventId), cancellationToken);
                return Ok(new ApiResponse<List<ArFrameDto>>
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

        // POST /api/events/{eventId}/frames/{frameId}/usage
        [Authorize]
        [HttpPost("{eventId:guid}/frames/{frameId:guid}/usage")]
        [ProducesResponseType(typeof(ApiResponse<object>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<object>>> RecordFrameUsage(
            Guid eventId,
            Guid frameId,
            CancellationToken cancellationToken)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        StatusCode = 401,
                        Message = "Unauthorized. Please log in again.",
                        Data = null,
                        ResponsedAt = DateTime.UtcNow
                    });
                }

                await _mediator.Send(new RecordFrameUsageCommand(eventId, frameId, userId), cancellationToken);

                return StatusCode(201, new ApiResponse<object>
                {
                    StatusCode = 201,
                    Message = "Frame usage recorded successfully.",
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
                    Message = "An error occurred while recording frame usage.",
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
        }
    }

    public class SendWishwallMessageRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
