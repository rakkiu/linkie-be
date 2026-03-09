using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Model.Auth.Login
{
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
    }

}
