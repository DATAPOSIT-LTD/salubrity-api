using Microsoft.EntityFrameworkCore;
using Salubrity.Domain.Entities.Rbac;
using Salubrity.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Salubrity.Application.Interfaces.Repositories.Rbac;

namespace Salubrity.Infrastructure.Repositories.Rbac
{
    public class RolePermissionGroupRepository : IRolePermissionGroupRepository
    {
        private readonly AppDbContext _dbContext;

        public RolePermissionGroupRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ───────────────────────────────────────────────
        // CRUD Methods
        // ───────────────────────────────────────────────

        public async Task<List<RolePermissionGroup>> GetAllRolePermissionGroupsAsync()
        {
            return await _dbContext.RolePermissionGroups
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<RolePermissionGroup> GetRolePermissionGroupByIdAsync(Guid id)
        {
            return await _dbContext.RolePermissionGroups
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<RolePermissionGroup> CreateRolePermissionGroupAsync(RolePermissionGroup rolePermissionGroup)
        {
            _dbContext.RolePermissionGroups.Add(rolePermissionGroup);
            await _dbContext.SaveChangesAsync();
            return rolePermissionGroup;
        }

        public async Task UpdateRolePermissionGroupAsync(RolePermissionGroup rolePermissionGroup)
        {
            _dbContext.RolePermissionGroups.Update(rolePermissionGroup);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteRolePermissionGroupAsync(RolePermissionGroup rolePermissionGroup)
        {
            _dbContext.RolePermissionGroups.Remove(rolePermissionGroup);
            await _dbContext.SaveChangesAsync();
        }

        // ───────────────────────────────────────────────
        //  RBAC-Specific Methods
        // ───────────────────────────────────────────────

        public async Task<List<PermissionGroup>> GetPermissionGroupsByRoleAsync(Guid roleId)
        {
            return await _dbContext.RolePermissionGroups
                .Where(rpg => rpg.RoleId == roleId)
                .Include(rpg => rpg.PermissionGroup)
                .Select(rpg => rpg.PermissionGroup)
                .ToListAsync();
        }

        public async Task AssignPermissionGroupToRoleAsync(Guid roleId, Guid permissionGroupId)
        {
            var entity = new RolePermissionGroup
            {
                RoleId = roleId,
                PermissionGroupId = permissionGroupId
            };

            _dbContext.RolePermissionGroups.Add(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UnassignAllPermissionGroupsAsync(Guid roleId)
        {
            var existing = await _dbContext.RolePermissionGroups
                .Where(rpg => rpg.RoleId == roleId)
                .ToListAsync();

            _dbContext.RolePermissionGroups.RemoveRange(existing);
            await _dbContext.SaveChangesAsync();
        }
    }
}
