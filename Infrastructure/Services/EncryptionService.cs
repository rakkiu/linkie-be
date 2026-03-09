using Application.Interfaces;
using Infrastructure.Security;

namespace Infrastructure.Services
{
    public class EncryptionService : IEncryptionService
    {
        public string Encrypt(string plainText) => EncryptionHelper.Encrypt(plainText);
        public string Decrypt(string cipherText) => EncryptionHelper.Decrypt(cipherText);
        public string EncryptDeterministic(string plainText) => EncryptionHelper.EncryptDeterministic(plainText);
        public string DecryptDeterministic(string cipherText) => EncryptionHelper.DecryptDeterministic(cipherText);
    }
}
