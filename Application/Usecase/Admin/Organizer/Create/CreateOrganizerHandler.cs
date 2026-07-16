using Domain.Entity;
using Domain.Enums;
using Domain.Interfaces;
using Application.Interfaces;
using MediatR;
using BCrypt.Net;

namespace Application.Usecase.Admin.Organizer.Create
{
    public record CreateOrganizerCommand(
        string Username,
        string Email,
        string Password,
        string DisplayName,
        Guid ManagedEventId,
        Domain.Enums.PlanTier PlanTier
    ) : IRequest<OrganizerDto>;

    public record OrganizerDto(
        Guid Id,
        string Username,
        string Email,
        string DisplayName,
        Guid ManagedEventId,
        string ManagedEventName,
        Domain.Enums.PlanTier PlanTier
    );

    public class CreateOrganizerHandler : IRequestHandler<CreateOrganizerCommand, OrganizerDto>
    {
        private readonly IUserRepository _userRepo;
        private readonly IOrganizerRepository _organizerRepo;
        private readonly IEncryptionService _encryption;

        public CreateOrganizerHandler(
            IUserRepository userRepo,
            IOrganizerRepository organizerRepo,
            IEncryptionService encryption)
        {
            _userRepo = userRepo;
            _organizerRepo = organizerRepo;
            _encryption = encryption;
        }

        public async Task<OrganizerDto> Handle(CreateOrganizerCommand request, CancellationToken cancellationToken)
        {
            // 1. Kiểm tra username (handle) unique
            var existsByUsername = await _organizerRepo.ExistsByUsernameAsync(request.Username, cancellationToken);
            if (existsByUsername)
                throw new InvalidOperationException($"Handle @{request.Username} đã được sử dụng. Vui lòng chọn handle khác.");

            // 2. Kiểm tra email unique
            var existingByEmail = await _userRepo.GetByEmailAsync(request.Email, cancellationToken);
            if (existingByEmail != null)
                throw new InvalidOperationException($"Email {request.Email} đã được đăng ký trong hệ thống.");

            // 3. Kiểm tra event tồn tại
            var managedEvent = await _organizerRepo.GetEventByIdAsync(request.ManagedEventId, cancellationToken)
                ?? throw new KeyNotFoundException($"Sự kiện với ID {request.ManagedEventId} không tồn tại.");

            // 4. Tạo tài khoản Organizer
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username.ToLower().Trim(),
                Email = _encryption.EncryptDeterministic(request.Email.ToLower().Trim()),
                Name = _encryption.Encrypt(request.DisplayName),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = UserRole.Organizer,
                ManagedEventId = request.ManagedEventId,
                PlanTier = request.PlanTier,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepo.AddAsync(newUser, cancellationToken);
            await _userRepo.SaveChangesAsync(cancellationToken);

            return new OrganizerDto(
                newUser.Id,
                newUser.Username!,
                request.Email,
                request.DisplayName,
                request.ManagedEventId,
                managedEvent.Name,
                request.PlanTier
            );
        }
    }
}
