using Application.Model.Auth.Login;
using MediatR;

namespace Application.Usecase.Auth.Login
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="IRequest&lt;LoginResultDto&gt;" />
    /// <seealso cref="IBaseRequest" />
    /// <seealso cref="IEquatable&lt;LoginCommand&gt;" />
    public record LoginCommand(string Email, string Password) : IRequest<LoginResponseDto>;
}
