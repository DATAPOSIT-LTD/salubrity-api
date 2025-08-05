// File: Application/Interfaces/Services/Lookups/ILookupService.cs

using Salubrity.Application.DTOs.Lookups;

public interface ILookupService
{
    Task<List<BaseLookupResponse>> GetAllAsync();
    Task<BaseLookupResponse?> FindByNameAsync(string name);
}
