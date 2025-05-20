using DataAccessLayer.ViewModels;

namespace BusinessLayer.Interface;

public interface IRolesAndPermission
{
    Task<List<RoleViewModel>> GetRolesAsync();
    Task<RoleViewModel> GetRoleByIdAsync(int roleid);
    Task<List<PermissionViewModel>> GetPermissionsByRoleIdAsync(int roleid);
     Task<bool> UpdatePermissionsAsync(PermissionUpdateRequest request); // New Method

}
