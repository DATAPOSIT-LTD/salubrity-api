using OtpNet;
using Salubrity.Application.Interfaces.Security;

namespace Salubrity.Infrastructure.Security
{
    public class TotpService : ITotpService
    {
        public string GenerateSecretKey()
        {
            var key = KeyGeneration.GenerateRandomKey(20); // 160-bit secret
            return Base32Encoding.ToString(key);
        }

        public string GenerateQrCodeUri(string email, string issuer, string secretKey)
        {
            return $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}" +
                   $"?secret={secretKey}&issuer={Uri.EscapeDataString(issuer)}&digits=6";
        }

        public bool VerifyCode(string secretKey, string code)
        {
            var totp = new Totp(Base32Encoding.ToBytes(secretKey));
            return totp.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
        }
    }
}
