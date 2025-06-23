using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Salubrity.Application.Interfaces.Security;

namespace Salubrity.Infrastructure.Security
{
    public class RsaKeyProvider : IKeyProvider
    {
        private const string KeyDir = "/etc/salubrity/keys";
        private const string PrivatePath = $"{KeyDir}/private.key";
        private const string PublicPath = $"{KeyDir}/public.key";

        public RsaSecurityKey GetPrivateKey()
        {
            EnsureKeysExist();
            var privateKey = Convert.FromBase64String(File.ReadAllText(PrivatePath));
            var rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(privateKey, out _);
            return new RsaSecurityKey(rsa);
        }

        public RsaSecurityKey GetPublicKey()
        {
            EnsureKeysExist();
            var publicKey = Convert.FromBase64String(File.ReadAllText(PublicPath));
            var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(publicKey, out _);
            return new RsaSecurityKey(rsa);
        }

        private void EnsureKeysExist()
        {
            if (File.Exists(PrivatePath) && File.Exists(PublicPath)) return;

            Directory.CreateDirectory(KeyDir);

            using var rsa = RSA.Create(2048);
            File.WriteAllText(PrivatePath, Convert.ToBase64String(rsa.ExportRSAPrivateKey()));
            File.WriteAllText(PublicPath, Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo()));
        }
    }
}
