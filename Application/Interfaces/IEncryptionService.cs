namespace Application.Interfaces
{
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
        string EncryptDeterministic(string plainText);
        string DecryptDeterministic(string cipherText);
    }
}
