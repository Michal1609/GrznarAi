using GrznarAi.Web.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public PermissionService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<bool> UserHasPermissionAsync(string userId, string permissionKey)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.UserPermissions
                .Include(up => up.ApplicationPermission)
                .AnyAsync(up => up.ApplicationUserId == userId && 
                                up.ApplicationPermission.Key == permissionKey);
        }

        public async Task<IEnumerable<ApplicationPermission>> GetUserPermissionsAsync(string userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.UserPermissions
                .Include(up => up.ApplicationPermission)
                .Where(up => up.ApplicationUserId == userId)
                .Select(up => up.ApplicationPermission)
                .ToListAsync();
        }

        public async Task<IEnumerable<ApplicationPermission>> GetAllPermissionsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.ApplicationPermissions
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<bool> AddUserPermissionAsync(string userId, string permissionKey)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var permission = await context.ApplicationPermissions
                    .FirstOrDefaultAsync(p => p.Key == permissionKey);
                    
                if (permission == null)
                    return false;
                
                // Kontrola, zda již uživatel nemá toto oprávnění
                var existingPermission = await context.UserPermissions
                    .FirstOrDefaultAsync(up => up.ApplicationUserId == userId && 
                                              up.ApplicationPermissionId == permission.Id);
                                              
                if (existingPermission != null)
                    return true; // Uživatel již má toto oprávnění
                
                // Přidání nového oprávnění
                var userPermission = new UserPermission
                {
                    ApplicationUserId = userId,
                    ApplicationPermissionId = permission.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                context.UserPermissions.Add(userPermission);
                await context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> RemoveUserPermissionAsync(string userId, string permissionKey)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var permission = await context.ApplicationPermissions
                    .FirstOrDefaultAsync(p => p.Key == permissionKey);
                    
                if (permission == null)
                    return false;
                
                var userPermission = await context.UserPermissions
                    .FirstOrDefaultAsync(up => up.ApplicationUserId == userId && 
                                              up.ApplicationPermissionId == permission.Id);
                                              
                if (userPermission == null)
                    return true; // Uživatel již nemá toto oprávnění
                
                // Odebrání oprávnění
                context.UserPermissions.Remove(userPermission);
                await context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<ApplicationPermission?> GetPermissionByKeyAsync(string key)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.ApplicationPermissions
                .FirstOrDefaultAsync(p => p.Key == key);
        }
    }
} 