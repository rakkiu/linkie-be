using Application.Model.Auth.Login;
using Application.Model.Auth.Token;
using Application.Usecase.Auth.ChangePassword;
using Application.Usecase.Auth.ForgotPassword;
using Application.Usecase.Auth.Login;
using Application.Usecase.Auth.Logout;
using Application.Usecase.Auth.Register;
using Application.Usecase.Auth.ResetPassword;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presentation.Common;
using System.Security.Claims;
namespace Presentation.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), 200)]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginCommand command)
        {
            var res = await _mediator.Send(command);
            return Ok(new ApiResponse<LoginResponseDto>
            {
                StatusCode = 200,
                Message = "Login successful",
                Data = res,
                ResponsedAt = DateTime.UtcNow
            });
        }

        [HttpPost("logout")]
        [ProducesResponseType(typeof (ApiResponse<>), 200)]
        public async Task<ActionResult<ApiResponse<object?>>> Logout([FromBody] LogoutCommand command, CancellationToken cancellationToken)
        {
            var res = await _mediator.Send(command, cancellationToken);
            return Ok(new ApiResponse<object?>
            {
                StatusCode = 200,
                Message = "Logout successful",
                Data = null,
                ResponsedAt = DateTime.UtcNow
            });
        }

        [HttpPost("refresh")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Refresh([FromBody] RefreshTokenRequest request)
        {
            var result = await _mediator.Send(new RefreshAccessTokenCommand(request));

            if (string.IsNullOrWhiteSpace(result.AccessToken) || string.IsNullOrWhiteSpace(result.RefreshToken))
            {
                return Unauthorized(new ApiResponse<object>
                {
                    StatusCode = 401,
                    Message = "Invalid or expired refresh token. Please login again.",
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }

            return Ok(new ApiResponse<LoginResponseDto>
            {
                StatusCode = 200,
                Message = "Tokens refreshed successfully",
                Data = result,
                ResponsedAt = DateTime.UtcNow
            });
        }

        // ---------------- FORGOT PASSWORD ----------------
        [HttpPost("forgetPassword")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<object>>> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        StatusCode = 400,
                        Message = "Email is missing.",
                        Data = null,
                        ResponsedAt = DateTime.UtcNow
                    });
                }

                var command = new ForgotPasswordCommand(request.Email);
                await _mediator.Send(command);

                return Ok(new ApiResponse<object>
                {
                    StatusCode = 200,
                    Message = "Password reset link has been sent to your email.",
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponse<object>
                {
                    StatusCode = 404,
                    Message = "Email not found in the system.",
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
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    StatusCode = 500,
                    Message = "An error occurred while processing your request.",
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
        }

        // ---------------- RESET PASSWORD ----------------
        [HttpPost("resetPassword")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        StatusCode = 400,
                        Message = "Invalid request data.",
                        Data = (object?)null,
                        ResponsedAt = DateTime.UtcNow
                    });
                }

                var command = new ResetPasswordCommand(request.Token, request.NewPassword);
                await _mediator.Send(command);

                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Password has been reset successfully.",
                    Data = (object?)null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "An error occurred while processing your request.",
                    Data = (object?)null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
        }

        // ---------------- CHANGE PASSWORD (FOR LOGGED-IN USER) ----------------
        [Authorize]
        [HttpPost("changePassword")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        StatusCode = 400,
                        Message = "Invalid request data.",
                        Data = (object?)null,
                        ResponsedAt = DateTime.UtcNow
                    });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new
                    {
                        StatusCode = 401,
                        Message = "Unauthorized. Please log in again.",
                        Data = (object?)null,
                        ResponsedAt = DateTime.UtcNow
                    });
                }

                var command = new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword);
                await _mediator.Send(command);

                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Password changed successfully.",
                    Data = (object?)null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = (object?)null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    StatusCode = 401,
                    Message = ex.Message,
                    Data = (object?)null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "An unexpected error occurred while changing password.",
                    Data = (object?)null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
        }

        // ---------------- REGISTER ----------------
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<RegisterResponseDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 409)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<ActionResult<ApiResponse<RegisterResponseDto>>> Register(
            [FromBody] RegisterCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _mediator.Send(command, cancellationToken);
                return StatusCode(201, new ApiResponse<RegisterResponseDto>
                {
                    StatusCode = 201,
                    Message = "Registration successful.",
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
            catch (InvalidOperationException ex)
            {
                return Conflict(new ApiResponse<object>
                {
                    StatusCode = 409,
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
                    Message = "An error occurred while processing your request.",
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                });
            }
        }
    }
}
