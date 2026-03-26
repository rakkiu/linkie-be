using Domain.Entity;
using Domain.Interfaces;
using Infrastructure.Identity;
using Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;

        public UserRepository(ApplicationDbContext db) => _db = db;

        public async Task AddAsync(User user, CancellationToken ct = default)
            => await _db.Users.AddAsync(user, ct);

        public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            var encryptedEmail = EncryptionHelper.EncryptDeterministic(email);
            var user = await _db.Users
                .Include(u => u.JwtTokens)
                .FirstOrDefaultAsync(u => u.Email == encryptedEmail, ct);

            if (user != null)
            {
                _db.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                DecryptUserSensitiveData(user);
            }

            return user;
        }

        public async Task<User?> GetByFirebaseUidAsync(string firebaseUid, CancellationToken ct = default)
        {
            var user = await _db.Users
                .Include(u => u.JwtTokens)
                .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid, ct);

            if (user != null)
            {
                _db.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                DecryptUserSensitiveData(user);
            }

            return user;
        }
        public async Task<User?> GetByIdWithoutDecryptAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        }
        public void UpdatePasswordOnly(User user)
        {
            var entry = _db.Entry(user);
            entry.Property(u => u.PasswordHash).IsModified = true;
        }

        private static void DecryptUserSensitiveData(User user)
        {
            user.Email = EncryptionHelper.DecryptDeterministic(user.Email);
            user.Name = EncryptionHelper.Decrypt(user.Name);
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
