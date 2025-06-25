using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Salubrity.Application.Interfaces.Rbac;
using Salubrity.Application.Interfaces.Services.Auth;
using Salubrity.Application.Interfaces.Services.Menus;
using Salubrity.Application.Interfaces.Services.Organizations;
using Salubrity.Application.Mappings;
using Salubrity.Application.Services.Auth;
using Salubrity.Application.Services.Menus;
using Salubrity.Application.Services.Organizations;
using Salubrity.Application.Services.Rbac;

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



            // Auth services
            services.AddScoped<IAuthService, AuthService>();
       

            return services;
        }
    }
}
