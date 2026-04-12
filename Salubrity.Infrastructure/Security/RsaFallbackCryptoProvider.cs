using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;

namespace Salubrity.Infrastructure.Security
{
    internal sealed class RsaFallbackCryptoProvider : ICryptoProvider
    {
        private readonly RsaSecurityKey _primary;
        private readonly RsaSecurityKey _fallback;
        private readonly ILogger _logger;

        public RsaFallbackCryptoProvider(
            RsaSecurityKey primary,
            RsaSecurityKey fallback,
            ILogger logger)
        {
            _primary = primary;
            _fallback = fallback;
            _logger = logger;
        }

        public object Create(string algorithm, params object[] args)
        {
            try
            {
                return CryptoProviderFactory.Default
                    .CreateForVerifying(_primary, algorithm);
            }
            catch
            {
                _logger.LogWarning(
                    "JWT verification failed with public.key — trying public.key-2"
                );

                return CryptoProviderFactory.Default
                    .CreateForVerifying(_fallback, algorithm);
            }
        }

        public bool IsSupportedAlgorithm(string algorithm, params object[] args)
            => algorithm.StartsWith("RS");

        public void Release(object cryptoInstance) { }
    }
}
