using Domain.Entity;

namespace Domain.Interfaces
{
    public interface IUserRepository
    {
        void UpdatePasswordOnly(User user);
        Task<User?> GetByIdWithoutDecryptAsync(Guid id, CancellationToken ct = default);
        Task<User> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
