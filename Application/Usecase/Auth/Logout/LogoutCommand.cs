using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Application.Usecase.Auth.Logout
{
    public record LogoutCommand (string refreshToken): IRequest<bool>;
   
}
