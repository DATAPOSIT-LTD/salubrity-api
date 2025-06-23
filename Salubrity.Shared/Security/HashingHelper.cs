using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Salubrity.Shared.Security.Config;

namespace Salubrity.Shared.Security;

public class HashingHelper
{
    private readonly int _saltSize;
    private readonly int _keySize;
    private readonly int _iterations;

    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public HashingHelper(IOptions<HashingSettings> options)
    {
        _saltSize = options.Value.SaltSize;
        _keySize = options.Value.KeySize;
        _iterations = options.Value.Iterations;
    }

    public string Hash(string input)
    {
        var salt = RandomNumberGenerator.GetBytes(_saltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(input, salt, _iterations, Algorithm, _keySize);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string input, string hashed)
    {
        var parts = hashed.Split('.', 2);
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);
        var storedHash = Convert.FromBase64String(parts[1]);
        var computedHash = Rfc2898DeriveBytes.Pbkdf2(input, salt, _iterations, Algorithm, _keySize);

        return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
    }
}
