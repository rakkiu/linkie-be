using Domain.Entity;

namespace Domain.Interfaces
{
    public interface IUserRepository
    {

        Task<User> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
