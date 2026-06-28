using Application.Usecase.Admin.Organizer.Create;
using Application.Usecase.Admin.Organizer.Delete;
using Application.Usecase.Admin.Organizer.GetAll;
using Application.Usecase.Admin.Organizer.Update;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/organizers")]
    [Authorize(Roles = "Admin")]
    public class OrganizerController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrganizerController(IMediator mediator) => _mediator = mediator;

        /// <summary>Lấy danh sách tất cả tài khoản Organizer</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetOrganizersQuery(), ct);
            return Ok(new { data = result, success = true });
        }

        /// <summary>Tạo tài khoản Organizer mới</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrganizerRequest request, CancellationToken ct)
        {
            try
            {
                var command = new CreateOrganizerCommand(
                    request.Username,
                    request.Email,
                    request.Password,
                    request.DisplayName,
                    request.ManagedEventId,
                    request.PlanTier
                );
                var result = await _mediator.Send(command, ct);
                return Ok(new { data = result, success = true, message = $"Tài khoản Organizer @{result.Username} đã được tạo thành công." });
            }
            catch (InvalidOperationException ex)
            {
                // Handle username/email duplicate → 409 Conflict
                return Conflict(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        /// <summary>Cập nhật tài khoản Organizer (tên hiển thị + sự kiện)</summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrganizerRequest request, CancellationToken ct)
        {
            try
            {
                await _mediator.Send(new UpdateOrganizerCommand(id, request.DisplayName, request.ManagedEventId, request.PlanTier), ct);
                return Ok(new { success = true, message = "Cập nhật tài khoản Organizer thành công." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        /// <summary>Xóa vĩnh viễn tài khoản Organizer</summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            try
            {
                await _mediator.Send(new DeleteOrganizerCommand(id), ct);
                return Ok(new { success = true, message = "Tài khoản Organizer đã bị xóa." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }
    }

    public record CreateOrganizerRequest(
        string Username,
        string Email,
        string Password,
        string DisplayName,
        Guid ManagedEventId,
        Domain.Enums.PlanTier PlanTier
    );

    public record UpdateOrganizerRequest(
        string DisplayName,
        Guid ManagedEventId,
        Domain.Enums.PlanTier PlanTier
    );
}
