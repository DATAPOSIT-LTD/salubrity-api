using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Salubrity.Application.Interfaces.Repositories.Lookups;
using Salubrity.Application.Interfaces.Repositories.Menus;
using Salubrity.Application.Interfaces.Repositories.Organizations;
using Salubrity.Application.Interfaces.Repositories.Rbac;
using Salubrity.Application.Interfaces.Repositories.Users;
using Salubrity.Application.Interfaces.Repositories.Camps;
using Salubrity.Application.Interfaces.Repositories.HealthcareServices;
using Salubrity.Infrastructure.Repositories.HealthcareServices;
using Salubrity.Infrastructure.Repositories.Camps;



using Salubrity.Application.Interfaces.Security;
using Salubrity.Domain.Seeders;
using Salubrity.Infrastructure.EventHandlers;
using Salubrity.Infrastructure.Persistence;
using Salubrity.Infrastructure.Repositories.Lookups;
using Salubrity.Infrastructure.Repositories.Menus;
using Salubrity.Infrastructure.Repositories.Organizations;
using Salubrity.Infrastructure.Repositories.Rbac;
using Salubrity.Infrastructure.Repositories.Users;
using Salubrity.Infrastructure.Repositories.Camps;
using Salubrity.Infrastructure.Security;
using Salubrity.Infrastructure.Seeders;

namespace Salubrity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");

        services.AddScoped<IRbacSeeder, RbacSeeder>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IPermissionGroupRepository, PermissionGroupRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRolePermissionGroupRepository, RolePermissionGroupRepository>();
        services.AddMediatR(typeof(AuditTrailEventHandler).Assembly);
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IKeyProvider, RsaKeyProvider>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITotpService, TotpService>();
        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<IMenuRoleRepository, MenuRoleRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IInsuranceProviderRepository, InsuranceProviderRepository>();
        services.AddScoped<IIndustryRepository, IndustryRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IServiceCategoryRepository, ServiceCategoryRepository>();
        services.AddScoped<IServiceSubcategoryRepository, ServiceSubcategoryRepository>();
        services.AddScoped<IServicePackageRepository, ServicePackageRepository>();
        services.AddScoped<ICampRepository, CampRepository>();







        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));


        return services;
    }
}
