using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IFirebaseService
    {
        Task<FirebaseUserInfo?> VerifyIdTokenAsync(string idToken);
    }

    public class FirebaseUserInfo
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Picture { get; set; }
        public string FirebaseUid { get; set; } = string.Empty;
    }
}
