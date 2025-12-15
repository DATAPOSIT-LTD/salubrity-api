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

        // Cached, paired keys (IMPORTANT)
        private RsaSecurityKey? _privateKey;
        private RsaSecurityKey? _publicKey;

        private const string KeyIdPrimary = "salubrity-rsa-1";
        private const string KeyIdFallback = "salubrity-rsa-2";

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
        // CORE LOGIC — LOAD ONE PAIR ONLY
        // ============================================================
        private void EnsureKeyPairLoaded()
        {
            if (_privateKey != null && _publicKey != null)
                return;

            EnsurePrimaryKeysExist();

            // 1️⃣ Try primary pair
            if (TryLoadPair(
                    PrivatePrimary,
                    PublicPrimary,
                    KeyIdPrimary,
                    out _privateKey,
                    out _publicKey))
            {
                _logger.LogInformation("JWT key pair loaded: PRIMARY");
                return;
            }

            // 2️⃣ Try fallback pair
            if (TryLoadPair(
                    PrivateFallback,
                    PublicFallback,
                    KeyIdFallback,
                    out _privateKey,
                    out _publicKey))
            {
                _logger.LogWarning("JWT key pair loaded: FALLBACK");
                return;
            }

            throw new CryptographicException(
                "Failed to load any valid RSA key pair for JWT signing/validation."
            );
        }

        // ============================================================
        // PAIR LOADER (ATOMIC)
        // ============================================================
        private static bool TryLoadPair(
            string privatePath,
            string publicPath,
            string keyId,
            out RsaSecurityKey privateKey,
            out RsaSecurityKey publicKey)
        {
            privateKey = null!;
            publicKey = null!;

            try
            {
                if (!File.Exists(privatePath) || !File.Exists(publicPath))
                    return false;

                var privateBytes = Convert.FromBase64String(File.ReadAllText(privatePath).Trim());
                var publicBytes = Convert.FromBase64String(File.ReadAllText(publicPath).Trim());

                var rsaPrivate = RSA.Create();
                rsaPrivate.ImportRSAPrivateKey(privateBytes, out _);

                var rsaPublic = RSA.Create();
                rsaPublic.ImportSubjectPublicKeyInfo(publicBytes, out _);

                privateKey = new RsaSecurityKey(rsaPrivate) { KeyId = keyId };
                publicKey = new RsaSecurityKey(rsaPublic) { KeyId = keyId };

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
        private static void EnsurePrimaryKeysExist()
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
