using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Salubrity.Application.Interfaces.Repositories;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.HealthcareServices;
using Salubrity.Application.Interfaces.Repositories.HomepageOverview;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Application.Interfaces.Repositories.Lookups;
using Salubrity.Application.Interfaces.Repositories.Menus;
using Salubrity.Application.Interfaces.Repositories.Organizations;
using Salubrity.Application.Interfaces.Repositories.Rbac;
using Salubrity.Application.Interfaces.Repositories.Users;
using Salubrity.Application.Interfaces.Security;
using Salubrity.Domain.Seeders;
using Salubrity.Infrastructure.Configuration;
using Salubrity.Infrastructure.EventHandlers;
using Salubrity.Infrastructure.Persistence;
using Salubrity.Infrastructure.Repositories;
using Salubrity.Infrastructure.Repositories.Employees;
using Salubrity.Infrastructure.Repositories.HealthCamps;
using Salubrity.Infrastructure.Repositories.HealthcareServices;
using Salubrity.Infrastructure.Repositories.HomepageOverview;
using Salubrity.Infrastructure.Repositories.IntakeForms;
using Salubrity.Infrastructure.Repositories.Lookups;
using Salubrity.Infrastructure.Repositories.Menus;
using Salubrity.Infrastructure.Repositories.Organizations;
using Salubrity.Infrastructure.Repositories.Rbac;
using Salubrity.Infrastructure.Repositories.Users;
using Salubrity.Infrastructure.Security;
using Salubrity.Infrastructure.Seeders;

using Salubrity.Application.Interfaces.Repositories;
using Salubrity.Infrastructure.Repositories.Employees;
using Salubrity.Infrastructure.Repositories;
using Salubrity.Infrastructure.Repositories.IntakeForms;
using Salubrity.Application.Interfaces.Repositories.HealthAssesment;
using Salubrity.Infrastructure.Repositories.HealthAssesment;
using Salubrity.Application.Interfaces.Repositories.Patients;
using Salubrity.Infrastructure.Repositories.Patients;
using Salubrity.Infrastructure.Persistence.Repositories.HealthCamps;



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
        services.AddScoped<IHealthCampRepository, HealthCampRepository>();
        services.AddScoped<IIntakeFormRepository, IntakeFormRepository>();
        services.AddScoped<IFormBuilderRepository, FormBuilderRepository>();
        services.AddScoped<IHealthCampManagementRepository, HealthCampManagementRepository>();
        services.AddScoped(typeof(ILookupRepository<>), typeof(LookupRepository<>));
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IIntakeFormRepository, IntakeFormRepository>();
        services.AddScoped<IPasswordGenerator, PasswordGenerator>();
        services.AddScoped<ISubcontractorRepository, SubcontractorRepository>();
        services.AddScoped<IQrCodeService, QrCodeService>();
        services.AddScoped<ITempPasswordService, TempPasswordServiceAdapter>();
        services.AddScoped<ICampTokenFactory, CampTokenFactoryAdapter>();
        services.Configure<EmailSettings>(config.GetSection(EmailSettings.SectionName));
        services.AddScoped<IHomepageOverviewRepository, HomepageOverviewRepository>();
        services.AddScoped<IHealthMetricThresholdRepository, HealthMetricThresholdRepository>();
        services.AddScoped<IUsersReadRepository, UsersReadRepository>();
        services.AddScoped<IUserRoleReadRepository, UserRoleReadRepository>();
        services.AddScoped<ISubcontractorReadRepository, SubcontractorReadRepository>();
        // Salubrity.Infrastructure/DependencyInjection.cs
        services.AddScoped<IEmployeeReadRepository, EmployeeReadRepository>();

        services.AddScoped<IEmailConfigurationRepository, EmailConfigurationRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<ISubcontractorCampAssignmentRepository, SubcontractorCampAssignmentRepository>();






        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));


        return services;
    }
}
