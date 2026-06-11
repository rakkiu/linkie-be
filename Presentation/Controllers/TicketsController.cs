using Application.Usecase.EventManagement.ToggleTicketVerification;
using Application.Usecase.Tickets.CheckUserTicket;
using Application.Usecase.Tickets.GetEventTickets;
using Application.Usecase.Tickets.ImportTickets;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Common;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TicketsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("api/admin/events/{eventId:guid}/tickets/import")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<ImportTicketsResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<ImportTicketsResponse>>> ImportTickets(
            Guid eventId,
            IFormFile file,
            CancellationToken cancellationToken)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        StatusCode = 400,
                        Message = "File is required",
                        Data = null,
                        ResponsedAt = DateTime.UtcNow
                    });
                }

                if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        StatusCode = 400,
                        Message = "Invalid file format. Only .xlsx files are supported",
                        Data = null,
                        ResponsedAt = DateTime.UtcNow
                    });
                }

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream, cancellationToken);
                stream.Position = 0;

                var command = new ImportTicketsCommand
                {
                    EventId = eventId,
                    FileStream = stream
                };

                var result = await _mediator.Send(command, cancellationToken);

                return Ok(new ApiResponse<ImportTicketsResponse>
                {
                    StatusCode = 200,
                    Message = result.Success ? "All tickets imported successfully" : "Import completed with errors",
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
                    Message = "An error occurred while importing tickets",
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
        }

        [HttpGet("api/events/{eventId:guid}/user/has-ticket")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CheckUserTicketResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<CheckUserTicketResponse>>> CheckUserTicket(
            Guid eventId,
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

                var query = new CheckUserTicketQuery
                {
                    EventId = eventId,
                    UserId = userId
                };

                var result = await _mediator.Send(query, cancellationToken);

                return Ok(new ApiResponse<CheckUserTicketResponse>
                {
                    StatusCode = 200,
                    Message = result.HasValidTicket ? "Valid ticket found" : "No valid ticket",
                    Data = result,
                    ResponsedAt = DateTime.UtcNow
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    StatusCode = 500,
                    Message = "An error occurred while checking ticket",
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
        }

        [HttpPatch("api/admin/events/{eventId:guid}/ticket-verification")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<ToggleTicketVerificationResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<ToggleTicketVerificationResponseDto>>> ToggleTicketVerification(
            Guid eventId,
            [FromBody] ToggleTicketVerificationRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var command = new ToggleTicketVerificationCommand(eventId, request.RequiresTicket);
                var result = await _mediator.Send(command, cancellationToken);

                return Ok(new ApiResponse<ToggleTicketVerificationResponseDto>
                {
                    StatusCode = 200,
                    Message = result.Message,
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
                    Message = "An error occurred while toggling ticket verification",
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
        }

        [HttpGet("api/admin/events/{eventId:guid}/tickets")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<GetEventTicketsResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<GetEventTicketsResponse>>> GetEventTickets(
            Guid eventId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? status = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = new GetEventTicketsQuery
                {
                    EventId = eventId,
                    Page = page,
                    PageSize = pageSize,
                    Status = status
                };

                var result = await _mediator.Send(query, cancellationToken);

                return Ok(new ApiResponse<GetEventTicketsResponse>
                {
                    StatusCode = 200,
                    Message = "Tickets retrieved successfully",
                    Data = result,
                    ResponsedAt = DateTime.UtcNow
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    StatusCode = 500,
                    Message = "An error occurred while retrieving tickets",
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
        }
    }

    public record ToggleTicketVerificationRequest(bool RequiresTicket);
}
