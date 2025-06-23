using System.Security.Cryptography;
using System.Text;

namespace Salubrity.Shared.Security;

public class RsaEncryptionHelper
{
    private const string PublicKeyPath = "keys/public_key.pem";
    private const string PrivateKeyPath = "keys/private_key.pem";

    private readonly RSA _publicKey;
    private readonly RSA _privateKey;

    public RsaEncryptionHelper()
    {
        Directory.CreateDirectory("keys");

        if (!File.Exists(PublicKeyPath) || !File.Exists(PrivateKeyPath))
        {
            GenerateAndSaveRsaKeys();
        }

        _publicKey = RSA.Create();
        _privateKey = RSA.Create();

        var pubKeyText = File.ReadAllText(PublicKeyPath);
        var privKeyText = File.ReadAllText(PrivateKeyPath);

        _publicKey.ImportFromPem(pubKeyText);
        _privateKey.ImportFromPem(privKeyText);
    }

    // -------------------------------------------------------
    // 🔐 ENCRYPTION
    // -------------------------------------------------------
    public string EncryptWithPublicKey(string plainText)
    {
        var bytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = _publicKey.Encrypt(bytes, RSAEncryptionPadding.OaepSHA256);
        return Convert.ToBase64String(encryptedBytes);
    }

    public string DecryptWithPrivateKey(string encryptedText)
    {
        var bytes = Convert.FromBase64String(encryptedText);
        var decryptedBytes = _privateKey.Decrypt(bytes, RSAEncryptionPadding.OaepSHA256);
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    // -------------------------------------------------------
    // 🖋️ DIGITAL SIGNATURE
    // -------------------------------------------------------
    public string SignWithPrivateKey(string plainText)
    {
        var data = Encoding.UTF8.GetBytes(plainText);
        var signature = _privateKey.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return Convert.ToBase64String(signature);
    }

    public bool VerifyWithPublicKey(string plainText, string base64Signature)
    {
        var data = Encoding.UTF8.GetBytes(plainText);
        var signature = Convert.FromBase64String(base64Signature);

        return _publicKey.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    // -------------------------------------------------------
    // 🔄 KEY GENERATION
    // -------------------------------------------------------
    private void GenerateAndSaveRsaKeys()
    {
        using var rsa = RSA.Create(4096);

        var publicKey = rsa.ExportRSAPublicKeyPem();
        var privateKey = rsa.ExportRSAPrivateKeyPem();

        File.WriteAllText(PublicKeyPath, publicKey);
        File.WriteAllText(PrivateKeyPath, privateKey);
    }
}
