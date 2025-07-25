﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Audit;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Domain.Entities.Identity;
using Salubrity.Domain.Entities.IntakeForms;
using Salubrity.Domain.Entities.Join;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Domain.Entities.Menus;
using Salubrity.Domain.Entities.Organizations;
using Salubrity.Domain.Entities.Rbac;
using Salubrity.Domain.Entities.Subcontractor;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Salubrity.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        private readonly IMediator _mediator;

        public AppDbContext(DbContextOptions<AppDbContext> options, IMediator mediator)
            : base(options)
        {
            _mediator = mediator;
        }

        // ─────────────────────────────────────
        // RBAC & Audit Tables
        // ─────────────────────────────────────
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<PermissionGroup> PermissionGroups => Set<PermissionGroup>();
        public DbSet<PermissionGroupPermission> PermissionGroupPermissions => Set<PermissionGroupPermission>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<RolePermissionGroup> RolePermissionGroups => Set<RolePermissionGroup>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Gender> Genders => Set<Gender>();
        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<OrganizationStatus> OrganizationStatuses => Set<OrganizationStatus>();
        public DbSet<Menu> Menus => Set<Menu>();
        public DbSet<MenuRole> MenuRoles => Set<MenuRole>();
        public DbSet<InsuranceProvider> InsuranceProviders => Set<InsuranceProvider>();
        public DbSet<Industry> Industries => Set<Industry>();
        public DbSet<Service> Services => Set<Service>();
        public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
        public DbSet<ServiceSubcategory> ServiceSubcategories => Set<ServiceSubcategory>();
        public DbSet<ServicePackage> ServicePackages => Set<ServicePackage>();
        public DbSet<IntakeForm> IntakeForms => Set<IntakeForm>();
        public DbSet<OrganizationInsuranceProvider> OrganizationInsuranceProviders => Set<OrganizationInsuranceProvider>();
        public DbSet<HealthCamp> HealthCamps => Set<HealthCamp>();
        public DbSet<HealthCampService> HealthCampServices => Set<HealthCampService>();
        public DbSet<HealthCampServiceAssignment> HealthCampServiceAssignments => Set<HealthCampServiceAssignment>();
        public DbSet<Subcontractor> Subcontractors => Set<Subcontractor>();
        public DbSet<SubcontractorSpecialty> SubcontractorSpecialties => Set<SubcontractorSpecialty>();
        public DbSet<SubcontractorHealthCampAssignment> SubcontractorHealthCampAssignments => Set<SubcontractorHealthCampAssignment>();
        public DbSet<SubcontractorHealthCampAssignmentStatus> SubcontractorHealthCampAssignmentStatuses => Set<SubcontractorHealthCampAssignmentStatus>();
        public DbSet<SubcontractorStatus> SubcontractorStatuses => Set<SubcontractorStatus>();
        public DbSet<SubcontractorRole> SubcontractorRoles => Set<SubcontractorRole>();
        public DbSet<SubcontractorRoleAssignment> SubcontractorRoleAssignments => Set<SubcontractorRoleAssignment>();
        public DbSet<HealthCampPackageItem> HealthCampPackageItems => Set<HealthCampPackageItem>();
        public DbSet<HealthCampParticipant> HealthCampParticipants => Set<HealthCampParticipant>();
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Language> Languages => Set<Language>();
        public DbSet<JobTitle> JobTitles => Set<JobTitle>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<UserLanguage> UserLanguages => Set<UserLanguage>();









        // ─────────────────────────────────────
        //  Model Configuration
        // ─────────────────────────────────────
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            base.OnModelCreating(modelBuilder);

            Console.WriteLine("STARTING MODEL CONFIG");

            try
            {
                modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            }
            catch (Exception ex)
            {
                Console.WriteLine("💥 Error applying configurations: " + ex);
                throw;
            }

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Phone)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(u => u.Name)
                .IsUnique();
            


            modelBuilder.Entity<Menu>()
                .HasIndex(m => m.Path)
                .IsUnique();

            modelBuilder.Entity<Menu>()
                .HasOne(m => m.Parent)
                .WithMany(m => m.Children)
                .HasForeignKey(m => m.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Menu>()
                .HasOne(m => m.RequiredPermission)
                .WithMany()
                .HasForeignKey(m => m.RequiredPermissionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MenuRole>()
                .HasIndex(mr => new { mr.MenuId, mr.RoleId })
                .IsUnique();

            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasIndex(p => p.Code)
                      .IsUnique();

            });

            modelBuilder.Entity<User>()
                .HasIndex(u => u.RefreshToken)
                .IsUnique(false); 

            modelBuilder.Entity<Industry>()
                .HasIndex(u => u.Name)
                .IsUnique();

            modelBuilder.Entity<ServiceCategory>()
                .HasIndex(u => u.Name)
                .IsUnique();

            modelBuilder.Entity<ServiceSubcategory>()
                .HasIndex(u => u.Name)
                .IsUnique();

            modelBuilder.Entity<UserLanguage>()
          .HasKey(ul => new { ul.UserId, ul.LanguageId });

            modelBuilder.Entity<UserLanguage>()
                .HasOne(ul => ul.User)
                .WithMany(u => u.UserLanguages)
                .HasForeignKey(ul => ul.UserId);

            modelBuilder.Entity<UserLanguage>()
                .HasOne(ul => ul.Language)
                .WithMany(l => l.UserLanguages)
                .HasForeignKey(ul => ul.LanguageId);

            modelBuilder.Entity<JobTitle>()
                .HasMany<Employee>()
                .WithOne(e => e.JobTitle)
                .HasForeignKey(e => e.JobTitleId);

            modelBuilder.Entity<Department>()
                .HasMany<Employee>()
                .WithOne(e => e.Department)
                .HasForeignKey(e => e.DepartmentId);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            ApplySoftDeleteFilter(modelBuilder);
        }

        private static void ApplySoftDeleteFilter(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseAuditableEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var isDeletedProp = Expression.Property(parameter, nameof(BaseAuditableEntity.IsDeleted));
                    var compare = Expression.Equal(isDeletedProp, Expression.Constant(false));
                    var lambda = Expression.Lambda(compare, parameter);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }

        // ─────────────────────────────────────
        // Domain Event & Audit Hooks
        // ─────────────────────────────────────
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await DispatchDomainEventsAsync(cancellationToken);
            ApplyAuditInfo();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyAuditInfo()
        {
            var entries = ChangeTracker.Entries<BaseAuditableEntity>();

            foreach (var entry in entries)
            {
                var now = DateTime.UtcNow;

                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = now;
                }
            }
        }

        private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
        {
            var domainEventEntities = ChangeTracker
                .Entries<IHasDomainEvents>()
                .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any())
                .ToList();

            foreach (var entry in domainEventEntities)
            {
                var entity = entry.Entity;
                var events = entity.DomainEvents.ToList();
                entity.ClearDomainEvents();

                foreach (var domainEvent in events)
                {
                    await _mediator.Publish(domainEvent, cancellationToken);
                }
            }
        }

    
    }
}
