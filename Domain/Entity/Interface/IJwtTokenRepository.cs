
namespace Domain.Interface
{
    public interface IJwtTokenRepository
    {
        Task DeleteAsync(string refreshToken);
        Task SaveChangeAsync(CancellationToken ct = default);
    }
}
