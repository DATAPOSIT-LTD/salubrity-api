
// File: Salubrity.Application/Interfaces/Services/HealthCamps/ITempPasswordService.cs
public interface ITempPasswordService
{
    string Generate(int length);
    string Hash(string plain);
}