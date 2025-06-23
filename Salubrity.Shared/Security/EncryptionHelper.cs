using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Salubrity.Shared.Security;

public class EncryptionHelper
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public EncryptionHelper(IConfiguration config)
    {
        var secretKey = config["Encryption:Key"];
        var iv = config["Encryption:IV"];

        if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(iv))
            throw new Exception("Encryption key and IV must be set in configuration.");

        _key = Encoding.UTF8.GetBytes(secretKey);
        _iv = Encoding.UTF8.GetBytes(iv);

        if (_key.Length != 32 || _iv.Length != 16)
            throw new Exception("Key must be 32 bytes (256-bit) and IV must be 16 bytes.");
    }

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return Convert.ToBase64String(cipherBytes);
    }

    public string Decrypt(string cipherText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var decryptor = aes.CreateDecryptor();
        var cipherBytes = Convert.FromBase64String(cipherText);
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
