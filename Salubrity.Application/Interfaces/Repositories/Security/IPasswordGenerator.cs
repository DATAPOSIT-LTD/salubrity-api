namespace Salubrity.Application.Interfaces.Security;

public interface IPasswordGenerator
{
    string Generate(int length = 12);
}
