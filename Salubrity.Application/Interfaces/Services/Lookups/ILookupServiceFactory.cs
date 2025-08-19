// File: Application/Interfaces/Services/Lookups/ILookupServiceFactory.cs

public interface ILookupServiceFactory
{
    ILookupService Resolve(string lookupName);
}
