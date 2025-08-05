// File: Application/Services/Lookups/LookupServiceFactory.cs

using System;
using Microsoft.Extensions.DependencyInjection;
using Salubrity.Application.Interfaces.Services.Lookups;
using Salubrity.Domain.Entities.Lookup;

public class LookupServiceFactory : ILookupServiceFactory
{
    private readonly IServiceProvider _provider;

    public LookupServiceFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public ILookupService Resolve(string lookupName)
    {
        return lookupName.ToLowerInvariant() switch
        {
            "languages" => _provider.GetRequiredService<GenericLookupService<Language>>(),
            "genders" => _provider.GetRequiredService<GenericLookupService<Gender>>(),
            "departments" => _provider.GetRequiredService<GenericLookupService<Department>>(),
            "jobtitles" => _provider.GetRequiredService<GenericLookupService<JobTitle>>(),
            _ => throw new NotImplementedException($"Lookup service for '{lookupName}' not registered.")
        };
    }
}
