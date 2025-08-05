// File: Salubrity.Infrastructure/Security/PasswordGenerator.cs

namespace Salubrity.Infrastructure.Security;

using Salubrity.Application.Interfaces.Security;

public class PasswordGenerator : IPasswordGenerator
{
    public string Generate(int length = 12)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@$%&*?";
        var random = new Random();
        return new string(Enumerable.Range(1, length).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }
}
