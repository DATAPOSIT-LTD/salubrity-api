namespace Salubrity.Shared.Security.Config;

public class HashingSettings
{
    public int SaltSize { get; set; } = 16;
    public int KeySize { get; set; } = 32;
    public int Iterations { get; set; } = 100_000;
}
