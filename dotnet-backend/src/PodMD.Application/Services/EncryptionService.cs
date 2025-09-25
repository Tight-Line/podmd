using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using PodMD.Application.Configuration;

namespace PodMD.Application.Services;

public class EncryptionService
{
    private readonly EncryptionSettings _settings;

    public EncryptionService(IOptions<EncryptionSettings> settings)
    {
        _settings = settings.Value;
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("Plain text cannot be null or empty", nameof(plainText));

        var key = Convert.FromBase64String(_settings.Key);
        if (key.Length != 32)
            throw new InvalidOperationException("Encryption key must be 32 bytes (256 bits) for AES-256");

        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();

        var iv = aes.IV;
        var plainBytes = Encoding.UTF8.GetBytes(plainText);

        using var cipher = aes.CreateEncryptor();
        var cipherBytes = cipher.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Combine IV + CipherText for storage
        var combined = new byte[iv.Length + cipherBytes.Length];
        Buffer.BlockCopy(iv, 0, combined, 0, iv.Length);
        Buffer.BlockCopy(cipherBytes, 0, combined, iv.Length, cipherBytes.Length);

        return Convert.ToBase64String(combined);
    }

    public string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            throw new ArgumentException("Encrypted text cannot be null or empty", nameof(encryptedText));

        var key = Convert.FromBase64String(_settings.Key);
        if (key.Length != 32)
            throw new InvalidOperationException("Encryption key must be 32 bytes (256 bits) for AES-256");

        var combined = Convert.FromBase64String(encryptedText);
        if (combined.Length < 16)
            throw new InvalidOperationException("Invalid encrypted data format");

        // Extract IV (first 16 bytes) and ciphertext
        var iv = new byte[16];
        var cipherBytes = new byte[combined.Length - 16];
        Buffer.BlockCopy(combined, 0, iv, 0, 16);
        Buffer.BlockCopy(combined, 16, cipherBytes, 0, cipherBytes.Length);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
