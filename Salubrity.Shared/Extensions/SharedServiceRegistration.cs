// Path: Salubrity.Shared/Extensions/SharedServiceRegistration.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Salubrity.Shared.Security;

namespace Salubrity.Shared.Extensions;

public static class SharedServiceRegistration
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<EncryptionHelper>();
        services.AddSingleton<HashingHelper>();
        services.AddSingleton<RsaEncryptionHelper>();

        return services;
    }
}
