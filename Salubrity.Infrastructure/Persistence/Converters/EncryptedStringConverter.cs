using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Salubrity.Shared.Security;

namespace Salubrity.Infrastructure.Persistence.Converters;

public class EncryptedStringConverter : ValueConverter<string?, string?>
{
    public EncryptedStringConverter(EncryptionHelper encryption)
        : base(
            v => v == null ? null : encryption.Encrypt(v),
            v => v == null ? null : SafeDecrypt(encryption, v))
    { }

    private static string? SafeDecrypt(EncryptionHelper enc, string? v)
    {
        if (v == null) return null;
        try { return enc.Decrypt(v); }
        catch { return v; }
    }
}
