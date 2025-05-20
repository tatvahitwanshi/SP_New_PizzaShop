using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Permission
{
    public int Id { get; set; }

    public int Roleid { get; set; }

    public bool? Canview { get; set; }

    public bool? Canaddedit { get; set; }

    public bool? Candelete { get; set; }

    public string? CreatedBy { get; set; }

    public string? EditedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? EditDate { get; set; }

    public int Permissionid { get; set; }

    public bool? IsEnable { get; set; }

    public virtual Permissionlist PermissionNavigation { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
