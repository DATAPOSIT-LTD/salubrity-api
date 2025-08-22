// File: Application/Services/Lookups/LookupServiceFactory.cs

using Microsoft.Extensions.DependencyInjection;
using Salubrity.Application.Lookups; //  include your registry

public class LookupServiceFactory : ILookupServiceFactory
{
    private readonly IServiceProvider _provider;

    public LookupServiceFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public ILookupService Resolve(string lookupName)
    {
        if (!LookupRegistry.LookupMap.TryGetValue(lookupName.ToLowerInvariant(), out var type))
            throw new NotImplementedException($"Lookup service for '{lookupName}' not registered.");

        var serviceType = typeof(GenericLookupService<>).MakeGenericType(type);
        return (ILookupService)_provider.GetRequiredService(serviceType);
    }
}
