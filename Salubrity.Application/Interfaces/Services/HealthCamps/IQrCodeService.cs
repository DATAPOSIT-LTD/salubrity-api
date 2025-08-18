
// File: Salubrity.Application/Interfaces/Services/HealthCamps/IQrCodeService.cs
public interface IQrCodeService
{
    // returns Base64 PNG
    string GenerateBase64Png(string content);
}
