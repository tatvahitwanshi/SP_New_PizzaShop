using System;
using System.Collections.Generic;

namespace PizzashopRMS.Models;

public partial class Permissionlist
{
    public int Permissionid { get; set; }

    public string? Permissionname { get; set; }

    public virtual ICollection<Permission> Permissions { get; } = new List<Permission>();
}
