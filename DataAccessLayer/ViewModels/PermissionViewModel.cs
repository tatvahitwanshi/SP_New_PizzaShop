namespace DataAccessLayer.ViewModels;

public class PermissionViewModel
{
    public int Id { get; set; }

    public int Roleid { get; set; }

    public string? Permissionname { get; set; }
    public bool? Canview { get; set; }

    public bool? Canaddedit { get; set; }

    public bool? Candelete { get; set; }

    public string? CreatedBy { get; set; }

    public string? EditedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? EditDate { get; set; }

    public int Permissionid { get; set; }

    public bool? IsEnable { get; set; }
}
