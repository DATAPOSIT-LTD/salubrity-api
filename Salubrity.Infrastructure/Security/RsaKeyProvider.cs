using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using Salubrity.Application.Interfaces.Security;

namespace Salubrity.Infrastructure.Security
{
    public class RsaKeyProvider : IKeyProvider
    {
        private const string KeyDir = "/var/lib/salubrity/keys";

        private const string PrivateKeyPath = $"{KeyDir}/private.key";
        private const string PublicKeyPath = $"{KeyDir}/public.key";

        private const string KeyId = "salubrity-rsa-1";

        private readonly ILogger<RsaKeyProvider> _logger;

        private RsaSecurityKey? _privateKey;
        private RsaSecurityKey? _publicKey;

        public RsaKeyProvider(ILogger<RsaKeyProvider> logger)
        {
            _logger = logger;
        }

        // ============================================================
        // PUBLIC API
        // ============================================================

        public RsaSecurityKey GetPrivateKey()
        {
            EnsureKeyPairLoaded();
            return _privateKey!;
        }

        public RsaSecurityKey GetPublicKey()
        {
            EnsureKeyPairLoaded();
            return _publicKey!;
        }

        // ============================================================
        // CORE LOGIC — LOAD SINGLE PAIR
        // ============================================================

        private void EnsureKeyPairLoaded()
        {
            if (_privateKey != null && _publicKey != null)
                return;

            EnsureKeysExist();

            try
            {
                var privateBytes = Convert.FromBase64String(
                    File.ReadAllText(PrivateKeyPath).Trim()
                );

                var publicBytes = Convert.FromBase64String(
                    File.ReadAllText(PublicKeyPath).Trim()
                );

                var rsaPrivate = RSA.Create();
                rsaPrivate.ImportRSAPrivateKey(privateBytes, out _);

                var rsaPublic = RSA.Create();
                rsaPublic.ImportSubjectPublicKeyInfo(publicBytes, out _);

                _privateKey = new RsaSecurityKey(rsaPrivate) { KeyId = KeyId };
                _publicKey = new RsaSecurityKey(rsaPublic) { KeyId = KeyId };

                _logger.LogInformation("JWT RSA key pair loaded (single-key mode)");
            }
            catch (Exception ex)
            {
                throw new CryptographicException(
                    "Failed to load RSA key pair for JWT signing/validation.",
                    ex
                );
            }
        }

        // ============================================================
        // KEY GENERATION (FIRST RUN ONLY)
        // ============================================================

        private static void EnsureKeysExist()
        {
            if (File.Exists(PrivateKeyPath) && File.Exists(PublicKeyPath))
                return;

            Directory.CreateDirectory(KeyDir);

            using var rsa = RSA.Create(2048);

            File.WriteAllText(
                PrivateKeyPath,
                Convert.ToBase64String(rsa.ExportRSAPrivateKey())
            );

            File.WriteAllText(
                PublicKeyPath,
                Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo())
            );
        }
    }
}
