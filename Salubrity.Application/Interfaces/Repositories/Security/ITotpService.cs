namespace Salubrity.Application.Interfaces.Security
{
    public interface ITotpService
    {
        string GenerateSecretKey();
        string GenerateQrCodeUri(string email, string issuer, string secretKey);
        bool VerifyCode(string secretKey, string code);
    }
}
