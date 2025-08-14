using System.Security.Cryptography;
using System.Text;

namespace PodMD.Api.Services;

public interface IProtectionService
{
    string Protect(string plainText);
    string Unprotect(string cipherText);
}

public class AesProtectionService : IProtectionService
{
    private readonly byte[] _encryptionKey;

    public AesProtectionService(string encryptionKey)
    {
        if (string.IsNullOrEmpty(encryptionKey))
            throw new InvalidOperationException("Encryption key not configured.");

        _encryptionKey = Convert.FromBase64String(encryptionKey);
        if (_encryptionKey.Length != 32)
            throw new InvalidOperationException("Encryption key must be 256 bits (32 bytes).");
    }

    public string Protect(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var result = new byte[aes.IV.Length + encryptedBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string Unprotect(string cipherText)
    {
        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;

        var iv = new byte[aes.BlockSize / 8];
        var cipher = new byte[fullCipher.Length - iv.Length];
        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        var decryptedBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

        return Encoding.UTF8.GetString(decryptedBytes);
    }
}