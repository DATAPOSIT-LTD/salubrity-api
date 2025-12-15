using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Salubrity.Application.Interfaces.Security;
using Microsoft.Extensions.Logging;

namespace Salubrity.Infrastructure.Security
{
    public class RsaKeyProvider : IKeyProvider
    {
        private const string KeyDir = "/var/lib/salubrity/keys";

        private const string PrivatePrimary = $"{KeyDir}/private.key";
        private const string PublicPrimary = $"{KeyDir}/public.key";

        private const string PrivateFallback = $"{KeyDir}/private.key-2";
        private const string PublicFallback = $"{KeyDir}/public.key-2";

        private readonly ILogger<RsaKeyProvider> _logger;

        public RsaKeyProvider(ILogger<RsaKeyProvider> logger)
        {
            _logger = logger;
        }

        // ============================================================
        // PRIVATE KEY
        // ============================================================
        public RsaSecurityKey GetPrivateKey()
        {
            EnsureKeysExist();

            if (TryLoadPrivateKey(PrivatePrimary, out var primaryKey))
            {
                _logger.LogInformation("JWT private key loaded from {Path}", PrivatePrimary);
                return primaryKey;
            }

            if (TryLoadPrivateKey(PrivateFallback, out var fallbackKey))
            {
                _logger.LogWarning(
                    "Primary JWT private key failed. Falling back to {Path}",
                    PrivateFallback
                );
                return fallbackKey;
            }

            throw new CryptographicException(
                "Failed to load JWT private key from both primary and fallback locations."
            );
        }

        // ============================================================
        // PUBLIC KEY
        // ============================================================
        public RsaSecurityKey GetPublicKey()
        {
            EnsureKeysExist();

            if (TryLoadPublicKey(PublicPrimary, out var primaryKey))
            {
                _logger.LogInformation("JWT public key loaded from {Path}", PublicPrimary);
                return primaryKey;
            }

            if (TryLoadPublicKey(PublicFallback, out var fallbackKey))
            {
                _logger.LogWarning(
                    "Primary JWT public key failed. Falling back to {Path}",
                    PublicFallback
                );
                return fallbackKey;
            }

            throw new CryptographicException(
                "Failed to load JWT public key from both primary and fallback locations."
            );
        }

        // ============================================================
        // INTERNAL LOADERS
        // ============================================================
        private static bool TryLoadPrivateKey(string path, out RsaSecurityKey key)
        {
            key = null!;

            try
            {
                if (!File.Exists(path)) return false;

                var raw = File.ReadAllText(path).Trim();
                var bytes = Convert.FromBase64String(raw);

                var rsa = RSA.Create();
                rsa.ImportRSAPrivateKey(bytes, out _);

                key = new RsaSecurityKey(rsa);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryLoadPublicKey(string path, out RsaSecurityKey key)
        {
            key = null!;

            try
            {
                if (!File.Exists(path)) return false;

                var raw = File.ReadAllText(path).Trim();
                var bytes = Convert.FromBase64String(raw);

                var rsa = RSA.Create();
                rsa.ImportSubjectPublicKeyInfo(bytes, out _);

                key = new RsaSecurityKey(rsa);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ============================================================
        // KEY GENERATION (PRIMARY ONLY)
        // ============================================================
        private static void EnsureKeysExist()
        {
            if (File.Exists(PrivatePrimary) && File.Exists(PublicPrimary))
                return;

            Directory.CreateDirectory(KeyDir);

            using var rsa = RSA.Create(2048);

            File.WriteAllText(
                PrivatePrimary,
                Convert.ToBase64String(rsa.ExportRSAPrivateKey())
            );

            File.WriteAllText(
                PublicPrimary,
                Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo())
            );
        }
    }
}
