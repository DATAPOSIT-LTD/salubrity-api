using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using FluentValidation.AspNetCore;

// ✅ Fix the validator import path
using Salubrity.Application.Services.Rbac;

using Salubrity.Application.Interfaces.Services.Auth;
using Salubrity.Application.Interfaces.Repositories.Rbac;
using Salubrity.Application.Services.Auth;
using Salubrity.Application.Interfaces.Rbac; // ✅ Needed for IJwtService, IPasswordHasher, etc.

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

            // Auth services
            services.AddScoped<IAuthService, AuthService>();
       

            return services;
        }
    }
}
