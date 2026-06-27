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

            // 2. Find or Create User (Optimistic Write)
            User? user = await _userRepo.GetByFirebaseUidAsync(firebaseUser.FirebaseUid, cancellationToken);

            if (user == null)
            {
                Console.WriteLine(">>> GoogleLoginHandler: Creating new user (optimistic)...");
                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = _encryptionService.EncryptDeterministic(firebaseUser.Email),
                    Name = _encryptionService.Encrypt(firebaseUser.Name),
                    FirebaseUid = firebaseUser.FirebaseUid,
                    Role = UserRole.Attendee,
                    IsEmailVerified = true,
                    CreatedAt = DateTime.UtcNow
                };

                user = await _userRepo.CreateOrGetGoogleUserAsync(newUser, firebaseUser.Email, cancellationToken)
                       ?? throw new InvalidOperationException("Failed to create or find user.");
            }
            else
            {
                Console.WriteLine(">>> GoogleLoginHandler: User found by FirebaseUid.");
            }

            string plainEmail = user.Email;
            string plainName = user.Name;

            // 3. Generate tokens (Ensure we use PLAIN TEXT email/name)
            Guid? managedEventId = null;
            string? planTier = null;
            if (user.Role == Domain.Enums.UserRole.Organizer)
            {
                managedEventId = user.ManagedEventId;
                planTier = user.PlanTier.ToString().ToLower();
            }

            var accessToken = _jwtService.GenerateAccessToken(user.Id, plainEmail, plainName, user.Role.ToString(), managedEventId, planTier);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var refreshTokenExpirationDays = _jwtService.GetRefreshTokenExpirationDays();

            // 4. Save only RefreshToken to DB (AccessToken is stateless)
            var tokenEntity = new JwtToken
            {
                Token = refreshToken,
                TokenType = "RefreshToken",
                ExpiresAt = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(refreshTokenExpirationDays), DateTimeKind.Unspecified),
                IsRevoked = false,
                UserId = user.Id
            };
            await _jwtTokenRepo.SaveTokenAsync(tokenEntity, cancellationToken);
            await _jwtTokenRepo.SaveChangeAsync(cancellationToken);

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
    }
}
