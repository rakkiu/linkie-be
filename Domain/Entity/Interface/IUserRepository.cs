namespace Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
