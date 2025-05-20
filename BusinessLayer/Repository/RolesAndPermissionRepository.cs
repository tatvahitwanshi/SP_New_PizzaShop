using BusinessLayer.Interface;
using DataAccessLayer.Models;
using DataAccessLayer.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Repository;

public class RolesAndPermissionRepository : IRolesAndPermission
{
    private readonly PizzaShopContext _db;

    // Constructor to initialize database context
    public RolesAndPermissionRepository(PizzaShopContext db)
    {
        _db = db;

    }

    // Retrieves all roles with their IDs
    public async Task<List<RoleViewModel>> GetRolesAsync()
    {
        return await _db.Roles
            .Select(r => new RoleViewModel { Rolename = r.Rolename, Roleid = r.Roleid })
            .ToListAsync();
    }

    // Retrieves a specific role by its ID
    public async Task<RoleViewModel> GetRoleByIdAsync(int roleid)
    {
        return await _db.Roles
            .Where(r => r.Roleid == roleid)
            .Select(r => new RoleViewModel { Roleid = r.Roleid, Rolename = r.Rolename })
            .FirstOrDefaultAsync();
    }

    // Retrieves permissions for a given role ID
    public async Task<List<PermissionViewModel>> GetPermissionsByRoleIdAsync(int roleid)
    {
        return await _db.Permissions
            .Where(p => p.Roleid == roleid)
            .Join(_db.Permissionlists,
                p => p.Permissionid,
                pl => pl.Permissionid,
                (p, pl) => new PermissionViewModel
                {
                    Id = p.Id,
                    Roleid = p.Roleid,
                    Canview = p.Canview,
                    Canaddedit = p.Canaddedit,
                    Candelete = p.Candelete,
                    Permissionid = p.Permissionid,
                    IsEnable = p.IsEnable,
                    Permissionname = pl.Permissionname // Fetching the permission name
                })
            .ToListAsync();
    }

    // Updates role permissions based on request data
    public async Task<bool> UpdatePermissionsAsync(PermissionUpdateRequest request)
    {
        try
        {
            var permissionsToUpdate = await _db.Permissions
                .Where(p => p.Roleid == request.RoleId)
                .ToListAsync();

            foreach (var permission in request.Permissions)
            {
                var dbPermission = permissionsToUpdate.FirstOrDefault(p => p.Permissionid == permission.Permissionid);
                if (dbPermission != null)
                {
                    dbPermission.IsEnable = permission.IsEnable;
                    dbPermission.Canview = permission.Canview;
                    dbPermission.Canaddedit = permission.Canaddedit;
                    dbPermission.Candelete = permission.Candelete;
                    dbPermission.EditDate = DateTime.Now;
                    dbPermission.EditedBy = "Admin"; // Change as needed
                }
            }

            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }


}
