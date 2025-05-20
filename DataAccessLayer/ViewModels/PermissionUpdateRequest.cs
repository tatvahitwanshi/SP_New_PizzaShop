namespace DataAccessLayer.ViewModels;

public class PermissionUpdateRequest
{
     public int RoleId { get; set; }
    public required List<PermissionViewModel> Permissions { get; set; }
}
