using Application.Model.Auth.Login;
using MediatR;
using Microsoft.AspNetCore.Http;
using Presentation.Common;
using Microsoft.AspNetCore.Mvc;
using Application.Usecase.Auth.Login;
using Application.Usecase.Auth.Logout;
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
    }
}
