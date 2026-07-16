using Domain.Entity;
using Domain.Enums;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Presentation.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/pricing-requests")]
    public class PricingRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public PricingRequestsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // POST /api/pricing-requests
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePricingRequestDto dto, CancellationToken ct)
        {
            if (dto == null) return BadRequest("Dữ liệu không hợp lệ.");
            if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.CompanyName) || string.IsNullOrEmpty(dto.PhoneNumber))
            {
                return BadRequest("Email, Tên doanh nghiệp và Số điện thoại là bắt buộc.");
            }

            var request = new PricingRequest
            {
                Email = dto.Email.Trim(),
                CompanyName = dto.CompanyName.Trim(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                Website = dto.Website?.Trim(),
                Fanpage = dto.Fanpage?.Trim(),
                PlanId = dto.PlanId.Trim(),
                Status = PricingRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.PricingRequests.Add(request);
            await _db.SaveChangesAsync(ct);

            return StatusCode(201, new ApiResponse<object>
            {
                StatusCode = 201,
                Message = "Yêu cầu đăng ký hợp tác đã được gửi thành công.",
                Data = new { id = request.Id, status = request.Status.ToString() },
                ResponsedAt = DateTime.UtcNow
            });
        }

        // GET /api/pricing-requests
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] string? status, CancellationToken ct)
        {
            var query = _db.PricingRequests.AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<PricingRequestStatus>(status, true, out var statusEnum))
            {
                query = query.Where(r => r.Status == statusEnum);
            }

            var list = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct);

            var result = list.Select(r => new PricingRequestDto
            {
                Id = r.Id,
                Email = r.Email,
                CompanyName = r.CompanyName,
                PhoneNumber = r.PhoneNumber,
                Website = r.Website,
                Fanpage = r.Fanpage,
                PlanId = r.PlanId,
                Status = r.Status.ToString(),
                CreatedAt = r.CreatedAt
            }).ToList();

            return Ok(new ApiResponse<List<PricingRequestDto>>
            {
                StatusCode = 200,
                Message = "Lấy danh sách yêu cầu thành công.",
                Data = result,
                ResponsedAt = DateTime.UtcNow
            });
        }

        // PUT /api/pricing-requests/{id}/status
        [HttpPut("{id:guid}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusDto dto, CancellationToken ct)
        {
            if (dto == null || !Enum.TryParse<PricingRequestStatus>(dto.Status, true, out var newStatus))
            {
                return BadRequest("Trạng thái không hợp lệ.");
            }

            var request = await _db.PricingRequests.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (request == null) return NotFound("Yêu cầu không tồn tại.");

            request.Status = newStatus;
            request.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            return Ok(new ApiResponse<object>
            {
                StatusCode = 200,
                Message = "Cập nhật trạng thái thành công.",
                Data = new { id = request.Id, status = request.Status.ToString() },
                ResponsedAt = DateTime.UtcNow
            });
        }

        // DELETE /api/pricing-requests/{id}
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var request = await _db.PricingRequests.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (request == null) return NotFound("Yêu cầu không tồn tại.");

            _db.PricingRequests.Remove(request);
            await _db.SaveChangesAsync(ct);

            return Ok(new ApiResponse<object>
            {
                StatusCode = 200,
                Message = "Xóa yêu cầu thành công.",
                Data = new { id = id },
                ResponsedAt = DateTime.UtcNow
            });
        }
    }

    public class CreatePricingRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Website { get; set; }
        public string? Fanpage { get; set; }
        public string PlanId { get; set; } = string.Empty;
    }

    public class UpdateStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }

    public class PricingRequestDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Website { get; set; }
        public string? Fanpage { get; set; }
        public string PlanId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
