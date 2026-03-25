using Application.Interfaces;
using Application.Model.Auth.Login;
using Domain.Entity;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Interface;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Usecase.Auth.GoogleLogin
{
    public class GoogleLoginHandler : IRequestHandler<GoogleLoginCommand, LoginResponseDto>
    {
        private readonly IFirebaseService _firebaseService;
        private readonly IUserRepository _userRepo;
        private readonly IJwtService _jwtService;
        private readonly IJwtTokenRepository _jwtTokenRepo;
        private readonly IEncryptionService _encryptionService;

        public GoogleLoginHandler(
            IFirebaseService firebaseService,
            IUserRepository userRepo,
            IJwtService jwtService,
            IJwtTokenRepository jwtTokenRepo,
            IEncryptionService encryptionService)
        {
            _firebaseService = firebaseService;
            _userRepo = userRepo;
            _jwtService = jwtService;
            _jwtTokenRepo = jwtTokenRepo;
            _encryptionService = encryptionService;
        }

        public async Task<LoginResponseDto> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine($">>> GoogleLoginHandler START");
            // 1. Verify token with Firebase
            var firebaseUser = await _firebaseService.VerifyIdTokenAsync(request.IdToken);
            if (firebaseUser == null)
            {
                throw new UnauthorizedAccessException("Invalid Google Token.");
            }

            // 2. Find or Create User
            // Phase 1: Try finding by FirebaseUid
            User? user = await _userRepo.GetByFirebaseUidAsync(firebaseUser.FirebaseUid, cancellationToken);
            string plainEmail = firebaseUser.Email;
            string plainName = firebaseUser.Name;

            if (user == null)
            {
                // Phase 2: Try finding by Email (to link existing traditional accounts)
                user = await _userRepo.GetByEmailAsync(firebaseUser.Email, cancellationToken);
                
                if (user == null)
                {
                    // Phase 3: Create new user
                    Console.WriteLine(">>> GoogleLoginHandler: Creating new user...");
                    user = new User
                    {
                        Id = Guid.NewGuid(),
                        Email = _encryptionService.EncryptDeterministic(firebaseUser.Email),
                        Name = _encryptionService.Encrypt(firebaseUser.Name),
                        FirebaseUid = firebaseUser.FirebaseUid,
                        Role = UserRole.Attendee,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _userRepo.AddAsync(user, cancellationToken);
                    await _userRepo.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    // Phase 4: Link existing user
                    Console.WriteLine(">>> GoogleLoginHandler: Linking existing user by email...");
                    user.FirebaseUid = firebaseUser.FirebaseUid;
                    // Note: user.Email and user.Name are already decrypted by repo here
                    plainEmail = user.Email;
                    plainName = user.Name;

                    // We need to re-encrypt them for the update if the Repo doesn't handle it
                    // But usually we only update the FirebaseUid column
                    await _userRepo.SaveChangesAsync(cancellationToken);
                }
            }
            else 
            {
                Console.WriteLine(">>> GoogleLoginHandler: User found by FirebaseUid.");
                plainEmail = user.Email;
                plainName = user.Name;
            }

            // 3. Generate tokens (Ensure we use PLAIN TEXT email/name)
            var accessToken = _jwtService.GenerateAccessToken(user.Id, plainEmail, plainName, user.Role.ToString());
            var refreshToken = _jwtService.GenerateRefreshToken();

            var accessTokenExpirationMinutes = _jwtService.GetAccessTokenExpirationMinutes();
            var refreshTokenExpirationDays = _jwtService.GetRefreshTokenExpirationDays();

            // 4. Save tokens to DB
            await SaveTokenAsync(accessToken, "AccessToken", user.Id, accessTokenExpirationMinutes, cancellationToken);
            await SaveTokenAsync(refreshToken, "RefreshToken", user.Id, refreshTokenExpirationDays * 24 * 60, cancellationToken);
            await _jwtTokenRepo.SaveChangeAsync(cancellationToken);

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        private async Task SaveTokenAsync(string token, string type, Guid userId, int expirationMinutes, CancellationToken ct)
        {
            var tokenEntity = new JwtToken
            {
                Token = token,
                TokenType = type,
                ExpiresAt = DateTime.SpecifyKind(DateTime.UtcNow.AddMinutes(expirationMinutes), DateTimeKind.Unspecified),
                IsRevoked = false,
                UserId = userId
            };
            await _jwtTokenRepo.SaveTokenAsync(tokenEntity, ct);
        }
    }
}
