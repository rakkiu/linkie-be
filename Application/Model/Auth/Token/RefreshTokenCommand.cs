using Application.Model.Auth.Login;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Model.Auth.Token
{
    public record RefreshAccessTokenCommand(RefreshTokenRequest token) : IRequest<LoginResponseDto>;
}
