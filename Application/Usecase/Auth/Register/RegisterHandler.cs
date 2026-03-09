using Application.Interfaces;
using Domain.Entity;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;

namespace Application.Usecase.Auth.Register
{
    public class RegisterHandler : IRequestHandler<RegisterCommand, RegisterResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IEncryptionService _encryption;

        public RegisterHandler(IUserRepository userRepository, IEncryptionService encryption)
        {
            _userRepository = userRepository;
            _encryption = encryption;
        }

        public async Task<RegisterResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Name is required.");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required.");

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                throw new ArgumentException("Password must be at least 6 characters.");

            var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existing != null)
                throw new InvalidOperationException("Email is already registered.");

            var user = new User
            {
                Name = _encryption.Encrypt(request.Name),
                Email = _encryption.EncryptDeterministic(request.Email),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = UserRole.Attendee,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            return new RegisterResponseDto
            {
                Id = user.Id,
                Name = request.Name,
                Email = request.Email,
                Role = user.Role.ToString()
            };
        }
    }
}
