using Microsoft.Extensions.DependencyInjection;
using Salubrity.Shared.Security;

namespace Salubrity.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        services.AddSingleton<EncryptionHelper>();
        services.AddSingleton<HashingHelper>();

        return services;
    }
}
