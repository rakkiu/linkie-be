using Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Usecase.Auth.ChangePassword
{
    public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Unit>
    {
        private readonly IUserRepository _userRepository;

        public ChangePasswordHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            {
                throw new ArgumentException("Current password is missing.");
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                throw new ArgumentException("New password is missing.");
            }


            // Check if new password is same as current password
            if (request.CurrentPassword == request.NewPassword)
            {
                throw new ArgumentException("New password must be different from the current password.");
            }

            // Validate new password format (at least 6 chars, contains uppercase, lowercase, number, special char)
            if (!IsValidPasswordFormat(request.NewPassword))
            {
                throw new ArgumentException("New password does not meet security requirements.");
            }

            // Get user WITHOUT decrypting sensitive data
            var user = await _userRepository.GetByIdWithoutDecryptAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

         

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Current password is incorrect.");
            }

            // Hash and update new password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            // Only update password field
            _userRepository.UpdatePasswordOnly(user);

            // Save changes
            await _userRepository.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }

        private bool IsValidPasswordFormat(string password)
        {
            // Password must be at least 6 characters
            if (password.Length < 6)
                return false;

            // Must contain at least one uppercase letter
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]"))
                return false;

            // Must contain at least one lowercase letter
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-z]"))
                return false;

            // Must contain at least one digit
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"\d"))
                return false;

            // Must contain at least one special character
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]"))
                return false;

            return true;
        }
    }

}
