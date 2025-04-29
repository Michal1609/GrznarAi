using GrznarAi.Web.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrznarAi.Web.Services
{
    public interface IPermissionService
    {
        Task<bool> UserHasPermissionAsync(string userId, string permissionKey);
        Task<IEnumerable<ApplicationPermission>> GetUserPermissionsAsync(string userId);
        Task<IEnumerable<ApplicationPermission>> GetAllPermissionsAsync();
        Task<bool> AddUserPermissionAsync(string userId, string permissionKey);
        Task<bool> RemoveUserPermissionAsync(string userId, string permissionKey);
        Task<ApplicationPermission?> GetPermissionByKeyAsync(string key);
    }
} 