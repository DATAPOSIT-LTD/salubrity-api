using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Salubrity.Application.Interfaces;
using Salubrity.Application.Interfaces.IntakeForms;
using Salubrity.Application.Interfaces.Rbac;
using Salubrity.Application.Interfaces.Security;
using Salubrity.Application.Interfaces.Services;
using Salubrity.Application.Interfaces.Services.Auth;
using Salubrity.Application.Interfaces.Services.Employee;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Application.Interfaces.Services.IntakeForms;
using Salubrity.Application.Interfaces.Services.Lookups;
using Salubrity.Application.Interfaces.Services.Menus;
using Salubrity.Application.Interfaces.Services.Organizations;
using Salubrity.Application.Mappings;
using Salubrity.Application.Services.Auth;
using Salubrity.Application.Services.EmployeeServices;
using Salubrity.Application.Services.Forms;
using Salubrity.Application.Services.HealthCamps;
using Salubrity.Application.Services.HealthcareServices;
using Salubrity.Application.Services.IntakeForms;
using Salubrity.Application.Services.Lookups;
using Salubrity.Application.Services.Menus;
using Salubrity.Application.Services.Notifications;
using Salubrity.Application.Services.Organizations;
using Salubrity.Application.Services.Rbac;
using Salubrity.Application.Services.Subcontractor;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Domain.Entities.Subcontractor;
using Salubrity.Infrastructure.Services;



namespace Salubrity.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
        {
            // Register validators
            services.AddValidatorsFromAssemblyContaining<CreateRoleDtoValidator>();
            services.AddFluentValidationAutoValidation();

            // RBAC services
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IPermissionGroupService, PermissionGroupService>();
            services.AddScoped<IRolePermissionGroupService, RolePermissionGroupService>();
            services.AddScoped<IUserRoleService, UserRoleService>();
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IMenuRoleService, MenuRoleService>();
            services.AddAutoMapper(typeof(MenuMappingProfile).Assembly);
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddAutoMapper(typeof(OrganizationMappingProfile).Assembly);
            services.AddAutoMapper(typeof(IndustryProfile).Assembly);
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddScoped<IInsuranceProviderService, InsuranceProviderService>();
            services.AddScoped<IIndustryService, IndustryService>();
            services.AddScoped<IServiceService, ServiceService>();
            services.AddScoped<IServiceCategoryService, ServiceCategoryService>();
            services.AddScoped<IServiceSubcategoryService, ServiceSubcategoryService>();
            services.AddScoped<IServicePackageService, ServicePackageService>();
            services.AddScoped<IHealthCampService, HealthCampService>();
            services.AddScoped<IIntakeFormService, IntakeFormService>();
            services.AddScoped<IPackageReferenceResolver, PackageReferenceResolverService>();
            services.AddScoped<IHealthCampManagementService, HealthCampManagementService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IFormService, FormService>();
            services.AddScoped<IFormBuilderService, FormBuilderService>();
            services.AddScoped<ILookupService, GenericLookupService<Gender>>();
            services.AddScoped<GenericLookupService<HealthCampStatus>>();
            services.AddScoped<GenericLookupService<Language>>();
            services.AddScoped<GenericLookupService<Department>>();
            services.AddScoped<GenericLookupService<JobTitle>>();
            services.AddScoped<ILookupServiceFactory, LookupServiceFactory>();
            services.AddScoped<IEmployeeTemplateService, EmployeeTemplateService>();
            services.AddScoped<ISubcontractorService, SubcontractorService>();
            services.AddScoped<GenericLookupService<SubcontractorRole>>();
            services.AddScoped<GenericLookupService<SubcontractorStatus>>();
            services.AddScoped<GenericLookupService<SubcontractorHealthCampAssignmentStatus>>();
            services.AddScoped<IEmailService, EmailService>();

            services.AddScoped<ITemplateRenderer, ScribanTemplateRenderer>();



            // Auth services
            services.AddScoped<IAuthService, AuthService>();


            return services;
        }
    }
}
