// File: Salubrity.Infrastructure/Security/Adapters/TempPasswordServiceAdapter.cs
using Salubrity.Application.Interfaces.Security;

public sealed class TempPasswordServiceAdapter : ITempPasswordService
{
    private readonly IPasswordGenerator _generator; // yours
    private readonly IPasswordHasher _hasher;       // yours

    public TempPasswordServiceAdapter(IPasswordGenerator generator, IPasswordHasher hasher)
    {
        _generator = generator;
        _hasher = hasher;
    }

    public string Generate(int length) => _generator.Generate(length); // uses your policy

    public string Hash(string plain) => _hasher.HashPassword(plain); // adapt to your method name
}
