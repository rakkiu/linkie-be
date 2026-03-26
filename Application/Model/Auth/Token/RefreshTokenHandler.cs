using Application.Interfaces;
using Application.Model.Auth.Login;
using Domain.Entity;
using Domain.Interface;
using Application.Model.Admin;
using Application.Model.WishwallAi;
using Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Model.Auth.Token
{
    public class RefreshAccessTokenHandler : IRequestHandler<RefreshAccessTokenCommand, LoginResponseDto>
    {
        /// <summary>
        /// The JWT service
        /// </summary>
        private readonly IJwtService _jwtService;
        /// <summary>
        /// The JWT token repository
        /// </summary>
        private readonly IJwtTokenRepository _jwtTokenRepository;
        /// <summary>
        /// The user repository
        /// </summary>
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshAccessTokenHandler"/> class.
        /// </summary>
        /// <param name="jwtService">The JWT service.</param>
        /// <param name="jwtTokenRepository">The JWT token repository.</param>
        /// <param name="userRepository">The user repository.</param>
        public RefreshAccessTokenHandler(IJwtService jwtService, IJwtTokenRepository jwtTokenRepository, IUserRepository userRepository)
        {
            _jwtService = jwtService;
            _jwtTokenRepository = jwtTokenRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Processes the refresh token request and returns new tokens if valid.
        /// Logic:
        /// - If RefreshToken is valid and not expired ? Generate new tokens, delete old tokens
        /// - If RefreshToken is expired ? Delete ALL tokens of user (force re-login)
        /// - If RefreshToken is revoked or not found ? Return empty result
        /// </summary>
        /// <param name="request">The refresh token command containing the client's tokens.</param>
        /// <param name="ct">Cancellation token for async operation.</param>
        /// <returns>
        /// Returns new AccessToken and RefreshToken if successful; otherwise, empty LoginResultDto.
        /// </returns>
        public async Task<LoginResponseDto> Handle(RefreshAccessTokenCommand request, CancellationToken ct)
        {
            // 1?? Validate input
            if (string.IsNullOrWhiteSpace(request.token.RefreshToken))
            {
                return new LoginResponseDto(); 
            }
            var storedRefreshToken = await _jwtTokenRepository.GetRefreshTokenAsync(request.token.RefreshToken, ct);

            if (storedRefreshToken == null || storedRefreshToken.IsRevoked)
            {
             
                return new LoginResponseDto();
            }

            var userId = storedRefreshToken.UserId;

            if (storedRefreshToken.ExpiresAt < DateTime.UtcNow)
            {
           

                
                await _jwtTokenRepository.RemoveTokenAsync(storedRefreshToken, ct);

                var allAccessTokens = await _jwtTokenRepository.GetAccessTokenByUserIdAsync(userId, ct);
                if (allAccessTokens != null)
                {
                    await _jwtTokenRepository.RemoveTokenAsync(allAccessTokens, ct);
                }

                return new LoginResponseDto();
            }

            var user = await _userRepository.GetByIdWithoutDecryptAsync(userId, ct);

            if (user == null)
            {
                await _jwtTokenRepository.RemoveTokenAsync(storedRefreshToken, ct);

                var allAccessTokens = await _jwtTokenRepository.GetAccessTokenByUserIdAsync(userId, ct);
                if (allAccessTokens != null)
                {
                    await _jwtTokenRepository.RemoveTokenAsync(allAccessTokens, ct);
                }

                return new LoginResponseDto();
            }

            var newAccessToken = _jwtService.GenerateAccessToken(user.Id, user.Email, user.Name, user.Role.ToString());
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            if (!string.IsNullOrWhiteSpace(request.token.AccessToken))
            {
                var specificAccessToken = await _jwtTokenRepository.GetByTokenAsync(request.token.AccessToken);
                if (specificAccessToken != null && specificAccessToken.UserId == userId)
                {
                    await _jwtTokenRepository.RemoveTokenAsync(specificAccessToken, ct);
                }
            }
            else
            {
                var oldAccessToken = await _jwtTokenRepository.GetAccessTokenByUserIdAsync(userId, ct);
                if (oldAccessToken != null)
                {
                    await _jwtTokenRepository.RemoveTokenAsync(oldAccessToken, ct);
                }
            }

            await _jwtTokenRepository.RemoveTokenAsync(storedRefreshToken, ct);

            var newAccessTokenEntity = new JwtToken
            {
                Token = newAccessToken,
                TokenType = "AccessToken",
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtService.GetAccessTokenExpirationMinutes()),
                IsRevoked = false,
                UserId = user.Id
            };
            await _jwtTokenRepository.SaveTokenAsync(newAccessTokenEntity, ct);

            var newRefreshTokenEntity = new JwtToken
            {
                Token = newRefreshToken,
                TokenType = "RefreshToken",
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtService.GetRefreshTokenExpirationDays()),
                IsRevoked = false,
                UserId = user.Id
            };
            await _jwtTokenRepository.SaveTokenAsync(newRefreshTokenEntity, ct);
            await _jwtTokenRepository.SaveChangeAsync(ct);

            return new LoginResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

    }
}
