// File: Application/Interfaces/Services/Lookups/ILookupServiceFactory.cs

using Salubrity.Application.DTOs.Lookups;

public interface ILookupServiceFactory
{
    ILookupService Resolve(string lookupName);
}
