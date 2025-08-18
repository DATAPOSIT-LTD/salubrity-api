// File: Salubrity.Infrastructure/Security/Adapters/QrCodeService.cs
using QRCoder;
using Salubrity.Application.Interfaces.Services.HealthCamps;

public sealed class QrCodeService : IQrCodeService
{
    public string GenerateBase64Png(string content)
    {
        using var gen = new QRCodeGenerator();
        using var data = gen.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);
        var png = new PngByteQRCode(data);
        var bytes = png.GetGraphic(8);
        return Convert.ToBase64String(bytes);
    }
}
