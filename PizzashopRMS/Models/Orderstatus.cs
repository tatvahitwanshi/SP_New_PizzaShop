using System;
using System.Collections.Generic;

namespace PizzashopRMS.Models;

public partial class Orderstatus
{
    public int Orderstatusid { get; set; }

    public string Statusname { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; } = new List<Order>();
}
